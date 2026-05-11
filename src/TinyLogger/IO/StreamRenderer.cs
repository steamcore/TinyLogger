using TinyLogger.Formatters;

namespace TinyLogger.IO;

/// <summary>
/// Renders log messages to a stream using the supplied log formatter.
/// </summary>
public class StreamRenderer : StreamRendererBase
{
	private readonly Func<StreamWriter> createStreamWriter;

	private StreamWriter? streamWriter;

	public StreamRenderer(ILogFormatter logFormatter, Stream stream)
		: base(logFormatter)
	{
		ArgumentNullException.ThrowIfNull(stream);

		createStreamWriter = () => new StreamWriter(stream);
	}

	public StreamRenderer(ILogFormatter logFormatter, StreamWriter streamWriter)
		: base(logFormatter)
	{
		ArgumentNullException.ThrowIfNull(streamWriter);

		createStreamWriter = () => streamWriter;
	}

	public StreamRenderer(ILogFormatter logFormatter, Func<StreamWriter> createStreamWriter)
		: base(logFormatter)
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
