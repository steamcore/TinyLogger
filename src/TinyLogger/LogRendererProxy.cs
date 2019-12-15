using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TinyLogger
{
	internal class LogRendererProxy : ILogRenderer, IDisposable
	{
		private readonly List<Channel<Func<TokenizedMessage>>> channels = new List<Channel<Func<TokenizedMessage>>>();
		private readonly List<Task> workerTasks = new List<Task>();
		private readonly IOptions<TinyLoggerOptions> options;

		private bool disposed;

		public LogRendererProxy(IOptions<TinyLoggerOptions> options)
		{
			this.options = options;

			foreach (var renderer in options.Value.Renderers)
			{
				var channel = Channel.CreateBounded<Func<TokenizedMessage>>(options.Value.MaxQueueDepth);

				channels.Add(channel);
				workerTasks.Add(RenderWorker(channel.Reader, renderer));
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			if (disposing)
			{
				foreach (var channel in channels)
				{
					channel.Writer.Complete();
				}

				Task.WhenAll(workerTasks).GetAwaiter().GetResult();
			}

			disposed = true;
		}

		public async Task Render(Func<TokenizedMessage> message)
		{
			if (disposed)
				return;

			foreach (var channel in channels)
			{
				if (channel.Writer.TryWrite(message))
					continue;

				if (KeepMessage(message))
				{
					await channel.Writer.WriteAsync(message).ConfigureAwait(false);
				}
			}
		}

		private bool KeepMessage(Func<TokenizedMessage> message)
		{
			return options.Value.QueueDepthExceededBehavior switch
			{
				QueueDepthExceededBehavior.KeepAll => true,
				QueueDepthExceededBehavior.DiscardAll => false,
				_ => message().LogLevel >= LogLevel.Warning
			};
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Must not crash but can't handle or log errors")]
		private static async Task RenderWorker(ChannelReader<Func<TokenizedMessage>> reader, ILogRenderer renderer)
		{
			while (await reader.WaitToReadAsync().ConfigureAwait(false))
			{
				while (reader.TryRead(out var message))
				{
					try
					{
						await renderer.Render(message).ConfigureAwait(false);
					}
					catch (Exception)
					{
					}
				}
			}
		}
	}
}
