using System.Threading.Channels;

namespace TinyLogger;

internal class LogRendererProxy(TinyLoggerOptions options)
	: ILogRenderer, IDisposable
{
#if NET9_0_OR_GREATER
	private readonly Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private readonly List<Channel<TokenizedMessage>> channels = [];
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

	public Task FlushAsync()
	{
		return Task.CompletedTask;
	}

	public async Task RenderAsync(TokenizedMessage message)
	{
		if (disposed)
		{
			return;
		}

		if (options.UseSynchronousWrites)
		{
			foreach (var renderer in options.Renderers)
			{
				await renderer.RenderAsync(message).ConfigureAwait(false);
			}

			return;
		}

		EnsureInitialized();

		for (var i = 0; i < channels.Count; i++)
		{
			var channel = channels[i];

			if (channel.Writer.TryWrite(message))
			{
				continue;
			}

			if (options.BackPressureArbiter is null || options.BackPressureArbiter(message))
			{
				await channel.Writer.WriteAsync(message).ConfigureAwait(false);
			}
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
				var channel = Channel.CreateBounded<TokenizedMessage>(options.MaxQueueDepth);

				channels.Add(channel);
				workerTasks.Add(RenderMessagesAsync(channel.Reader, renderer));
			}

			initialized = true;
		}
	}

	private static async Task RenderMessagesAsync(ChannelReader<TokenizedMessage> reader, ILogRenderer renderer)
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
			}

			await renderer.FlushAsync();
		}
	}
}
