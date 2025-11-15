using System.Text;

namespace TinyLogger.IO;

/// <summary>
/// Renders log messages in plain text to a stream.
/// </summary>
public class StreamRenderer : ILogRenderer, IDisposable
{
	private readonly Func<StreamWriter> createStreamWriter;
	private readonly Lock streamWriterLock = new();

	private bool disposed;
	private StreamWriter? streamWriter;

	public StreamRenderer(Stream stream)
		: this(new Func<StreamWriter>(() => new StreamWriter(stream)))
	{
	}

	public StreamRenderer(StreamWriter streamWriter)
		: this(new Func<StreamWriter>(() => streamWriter))
	{
	}

	public StreamRenderer(Func<StreamWriter> createStreamWriter)
	{
		ArgumentNullException.ThrowIfNull(createStreamWriter);

		this.createStreamWriter = createStreamWriter;
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
			streamWriter?.Dispose();
		}

		disposed = true;
	}

	public Task FlushAsync()
	{
		return streamWriter?.FlushAsync() ?? Task.CompletedTask;
	}

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

		await sb.Value.WriteToStreamWriterAsync(GetStreamWriter()).ConfigureAwait(false);
	}

	private StreamWriter GetStreamWriter()
	{
		if (streamWriter != null)
		{
			return streamWriter;
		}

		lock (streamWriterLock)
		{
			return streamWriter ??= createStreamWriter();
		}
	}
}
