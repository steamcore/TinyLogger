namespace TinyLogger.IO;

/// <summary>
/// Renders log messages in plain text to a file.
/// </summary>
public class FileRenderer : StreamRenderer
{
	public FileRenderer(string fileName)
		: this(fileName, LogFileMode.Append)
	{
	}

	public FileRenderer(string fileName, LogFileMode logFileMode)
		: base(new Func<StreamWriter>(() => new StreamWriter(LogFile.OpenFile(fileName, logFileMode))))
	{
	}
}
