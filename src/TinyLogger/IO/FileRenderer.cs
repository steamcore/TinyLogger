namespace TinyLogger.IO;

/// <summary>
/// Render messages in plain text to a file, optionally with a rolling filename.
/// </summary>
public class FileRenderer(Func<string> getFileName, LogFileMode logFileMode)
	: StreamRendererBase
{
	private string? openFileName;
	private StreamWriter? streamWriter;

	public FileRenderer(string fileName)
		: this(() => fileName, LogFileMode.Append)
	{
	}

	public FileRenderer(string fileName, LogFileMode logFileMode)
		: this(() => fileName, logFileMode)
	{
	}

	public FileRenderer(Func<string> getFileName)
		: this(getFileName, LogFileMode.Append)
	{
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

	protected override async Task<StreamWriter> GetStreamWriterAsync()
	{
		var fileName = getFileName();

		if (streamWriter != null && (openFileName != fileName || !File.Exists(openFileName)))
		{
			await streamWriter.FlushAsync().ConfigureAwait(false);
			await streamWriter.DisposeAsync().ConfigureAwait(false);
			streamWriter = null;
		}

		openFileName = fileName;

		return streamWriter ??= new StreamWriter(LogFile.OpenFile(fileName, logFileMode));
	}
}
