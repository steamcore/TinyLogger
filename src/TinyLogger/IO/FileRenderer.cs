namespace TinyLogger.IO;

/// <summary>
/// Renders log messages in plain text to a file.
/// </summary>
public class FileRenderer(string fileName, LogFileMode logFileMode)
	: StreamRenderer(new Func<StreamWriter>(() => new StreamWriter(LogFile.OpenFile(fileName, logFileMode))))
{
	public FileRenderer(string fileName)
		: this(fileName, LogFileMode.Append)
	{
	}
}
