using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace TinyLogger;

internal class LogRendererProxy(TinyLoggerOptions options)
	: ILogRenderer, IDisposable
{
	private readonly object _lock = new();
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

	public Task Flush()
	{
		return Task.CompletedTask;
	}

	public async Task Render(TokenizedMessage message)
	{
		if (disposed)
			return;

		if (options.UseSynchronousWrites)
		{
			foreach (var renderer in options.Renderers)
			{
				await renderer.Render(message).ConfigureAwait(false);
			}

			return;
		}

		EnsureInitialized();

		for (var i = 0; i < channels.Count; i++)
		{
			var channel = channels[i];

			if (channel.Writer.TryWrite(message))
				continue;

			if (options.BackPressureArbiter is null || options.BackPressureArbiter(message))
			{
				await channel.Writer.WriteAsync(message).ConfigureAwait(false);
			}
		}
	}

	private void EnsureInitialized()
	{
		if (initialized)
			return;

		lock (_lock)
		{
			if (initialized)
				return;

			foreach (var renderer in options.Renderers)
			{
				var channel = Channel.CreateBounded<TokenizedMessage>(options.MaxQueueDepth);

				channels.Add(channel);
				workerTasks.Add(RenderWorker(channel.Reader, renderer));
			}

			initialized = true;
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
					// Swallow render errors, we can't log errors or it could become an infinite loop
				}
			}

			await renderer.Flush();
		}
	}
}
