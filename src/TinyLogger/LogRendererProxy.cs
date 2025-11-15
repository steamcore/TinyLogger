using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace TinyLogger;

internal class LogRendererProxy(TinyLoggerOptions options)
	: IDisposable
{
	private readonly Lock _lock = new();
	private readonly List<Channel<PooledTokenizedMessage>> channels = [];
	private readonly List<Task> workerTasks = [];
	private bool disposed;
	private bool initialized;

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposed)
		{
			return;
		}

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

	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Pooled object is disposed by channel worker")]
	public void Render(string categoryName, LogLevel logLevel, Action<IList<MessageToken>> populateMessage, Action<IReadOnlyList<MessageToken>, IList<MessageToken>> populateLogMessage)
	{
		if (disposed)
		{
			return;
		}

		if (options.UseSynchronousWrites)
		{
			using var tokenizedMessage = new PooledTokenizedMessage(categoryName, logLevel, populateMessage, populateLogMessage);

			foreach (var renderer in options.Renderers)
			{
				renderer.RenderAsync(tokenizedMessage).ConfigureAwait(false).GetAwaiter().GetResult();
			}

			return;
		}

		EnsureInitialized();

		foreach (var channel in channels)
		{
			var tokenizedMessage = new PooledTokenizedMessage(categoryName, logLevel, populateMessage, populateLogMessage);

			if (channel.Writer.TryWrite(tokenizedMessage))
			{
				continue;
			}

			if (options.BackPressureArbiter is null || options.BackPressureArbiter(tokenizedMessage))
			{
				channel.Writer.WriteAsync(tokenizedMessage).AsTask().ConfigureAwait(false).GetAwaiter().GetResult();

				continue;
			}

			tokenizedMessage.Dispose();
		}
	}

	private void EnsureInitialized()
	{
		if (initialized)
		{
			return;
		}

		lock (_lock)
		{
			if (initialized)
			{
				return;
			}

			foreach (var renderer in options.Renderers)
			{
				var channel = Channel.CreateBounded<PooledTokenizedMessage>(options.MaxQueueDepth);

				channels.Add(channel);
				workerTasks.Add(RenderMessagesAsync(channel.Reader, renderer));
			}

			initialized = true;
		}
	}

	private static async Task RenderMessagesAsync(ChannelReader<PooledTokenizedMessage> reader, ILogRenderer renderer)
	{
		while (await reader.WaitToReadAsync().ConfigureAwait(false))
		{
			while (reader.TryRead(out var message))
			{
				try
				{
					await renderer.RenderAsync(message).ConfigureAwait(false);
				}
				catch
				{
					// Swallow render errors, we can't log errors or it could become an infinite loop
				}
				finally
				{
					message.Dispose();
				}
			}

			await renderer.FlushAsync();
		}
	}
}
