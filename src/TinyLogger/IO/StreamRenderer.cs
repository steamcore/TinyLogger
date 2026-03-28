namespace TinyLogger.IO;

/// <summary>
/// Renders log messages in plain text to a stream.
/// </summary>
public class StreamRenderer : StreamRendererBase
{
	private readonly Func<StreamWriter> createStreamWriter;

	private StreamWriter? streamWriter;

	public StreamRenderer(Stream stream)
	{
		ArgumentNullException.ThrowIfNull(stream);

		createStreamWriter = () => new StreamWriter(stream);
	}

	public StreamRenderer(StreamWriter streamWriter)
	{
		ArgumentNullException.ThrowIfNull(streamWriter);

		createStreamWriter = () => streamWriter;
	}

	public StreamRenderer(Func<StreamWriter> createStreamWriter)
	{
		ArgumentNullException.ThrowIfNull(createStreamWriter);

		this.createStreamWriter = createStreamWriter;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		if (disposing)
		{
			streamWriter?.Dispose();
		}
	}

	override public void Flush()
	{
		streamWriter?.Flush();
	}

	public override async Task FlushAsync()
	{
		if (streamWriter != null)
		{
			await streamWriter.FlushAsync().ConfigureAwait(false);
		}
	}

	protected override Task<StreamWriter> GetStreamWriterAsync()
	{
		return Task.FromResult(streamWriter ??= createStreamWriter());
	}
}
