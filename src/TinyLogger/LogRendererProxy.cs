using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace TinyLogger
{
	internal class LogRendererProxy : ILogRenderer, IDisposable
	{
		private readonly List<Channel<TokenizedMessage>> channels = new List<Channel<TokenizedMessage>>();
		private readonly List<Task> workerTasks = new List<Task>();
		private readonly TinyLoggerOptions options;

		private bool disposed;

		public LogRendererProxy(TinyLoggerOptions options)
		{
			this.options = options;

			foreach (var renderer in options.Renderers)
			{
				var channel = Channel.CreateBounded<TokenizedMessage>(options.MaxQueueDepth);

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

		public async Task Render(TokenizedMessage message)
		{
			if (disposed)
				return;

			foreach (var channel in channels)
			{
				if (channel.Writer.TryWrite(message))
					continue;

				if (options.BackPressureArbiter is null || options.BackPressureArbiter(message))
				{
					await channel.Writer.WriteAsync(message).ConfigureAwait(false);
				}
			}
		}

		[SuppressMessage("Design", "RCS1075:Avoid empty catch clause that catches System.Exception.", Justification = "Must not crash but can't handle or log errors")]
		private static async Task RenderWorker(ChannelReader<TokenizedMessage> reader, ILogRenderer renderer)
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
