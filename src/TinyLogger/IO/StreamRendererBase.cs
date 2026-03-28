using System.Text;

namespace TinyLogger.IO;

/// <summary>
/// Renders log messages in plain text to a stream.
/// </summary>
public abstract class StreamRendererBase : ILogRenderer, IDisposable
{
	private bool disposed;

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
			Flush();
		}

		disposed = true;
	}

	public abstract void Flush();
	public abstract Task FlushAsync();

	public async Task RenderAsync(TokenizedMessage message)
	{
		if (disposed || message is null)
		{
			return;
		}

		using var sb = Pooling.RentStringBuilder();

		for (var i = 0; i < message.MessageTokens.Count; i++)
		{
			var token = message.MessageTokens[i];

			token.Write(sb.Value);
		}

		var streamWriter = await GetStreamWriterAsync().ConfigureAwait(false);

		await sb.Value.WriteToStreamWriterAsync(streamWriter).ConfigureAwait(false);
	}

	protected abstract Task<StreamWriter> GetStreamWriterAsync();
}
