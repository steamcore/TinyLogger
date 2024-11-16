using System.Text;

namespace TinyLogger.IO;

/// <summary>
/// Renders log messages in plain text to a stream.
/// </summary>
public class StreamRenderer : ILogRenderer, IDisposable
{
	private readonly Func<StreamWriter> createStreamWriter;
#if NET9_0_OR_GREATER
	private readonly Lock streamWriterLock = new();
#else
	private readonly object streamWriterLock = new();
#endif

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
#if NET
		ArgumentNullException.ThrowIfNull(createStreamWriter);
#else
		if (createStreamWriter is null)
		{
			throw new ArgumentNullException(nameof(createStreamWriter));
		}
#endif

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

	public Task Flush()
	{
		return streamWriter?.FlushAsync() ?? Task.CompletedTask;
	}

	public async Task Render(TokenizedMessage message)
	{
		if (disposed || message is null)
		{
			return;
		}

		using var sb = Pooling.RentStringBuilder();
		using var messageTokens = message.RentMessageTokenList();

		for (var i = 0; i < messageTokens.Value.Count; i++)
		{
			var token = messageTokens.Value[i];

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
