using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace TinyLogger
{
	internal class LogRendererProxy : ILogRenderer, IDisposable
	{
		private readonly ReactiveEnumerable<Func<TokenizedMessage>> messageEnumerable;
		private readonly Task renderTask;

		private bool disposed;

		public LogRendererProxy(TinyLoggerOptions options)
		{
			messageEnumerable = new ReactiveEnumerable<Func<TokenizedMessage>>(
				options.MaxQueueDepth,
				message => MessageArbiter(options, message)
			);

			renderTask = Task.WhenAll(
				options.Renderers
					.Select(renderer => RenderWorker(messageEnumerable, renderer)
				)
			);
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
				messageEnumerable.OnComplete();
				renderTask.ConfigureAwait(false).GetAwaiter().GetResult();
			}

			disposed = true;
		}

		public Task Render(Func<TokenizedMessage> message)
		{
			if (!disposed)
			{
				messageEnumerable.OnNext(message);
			}

			return Task.CompletedTask;
		}

		private static bool MessageArbiter(TinyLoggerOptions options, Func<TokenizedMessage> message)
		{
			if (options.QueueDepthExceededBehavior == QueueDepthExceededBehavior.KeepAll)
			{
				return true;
			}

			if (options.QueueDepthExceededBehavior == QueueDepthExceededBehavior.DiscardAll)
			{
				return false;
			}

			return message().LogLevel >= LogLevel.Warning;
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Must not crash but can't handle or log errors")]
		private static async Task RenderWorker(IAsyncEnumerable<Func<TokenizedMessage>> messages, ILogRenderer renderer)
		{
			await Task.Yield();

			await foreach (var message in messages)
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
