namespace TinyLogger.IO;

/// <summary>
/// Renders log messages in plain text to a file.
/// </summary>
public class FileRenderer(string fileName, LogFileMode logFileMode) : StreamRendererBase
{
	private StreamWriter? streamWriter;

	public FileRenderer(string fileName)
		: this(fileName, LogFileMode.Append)
	{
	}

	override protected void Dispose(bool disposing)
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
		// Recreate the StreamWriter if the file has been deleted7
		if (streamWriter != null && !File.Exists(fileName))
		{
			await streamWriter.FlushAsync().ConfigureAwait(false);
			await streamWriter.DisposeAsync().ConfigureAwait(false);
			streamWriter = null;
		}

		return streamWriter ??= new StreamWriter(LogFile.OpenFile(fileName, logFileMode));
	}
}
