using System.Text;
using TinyLogger.Formatters;

namespace TinyLogger.IO;

/// <summary>
/// Renders log messages to a stream using the supplied log formatter.
/// </summary>
public abstract class StreamRendererBase : ILogRenderer, IDisposable
{
	private readonly ILogFormatter logFormatter;

	private bool disposed;

	protected StreamRendererBase(ILogFormatter logFormatter)
	{
		ArgumentNullException.ThrowIfNull(logFormatter);
		this.logFormatter = logFormatter;
	}

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

		logFormatter.Format(message, sb.Value);

		var streamWriter = await GetStreamWriterAsync().ConfigureAwait(false);

		await sb.Value.WriteToStreamWriterAsync(streamWriter).ConfigureAwait(false);
	}

	protected abstract Task<StreamWriter> GetStreamWriterAsync();
}
