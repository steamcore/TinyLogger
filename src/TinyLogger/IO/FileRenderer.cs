namespace TinyLogger.IO;

/// <summary>
/// Render messages in plain text to a file, optionally with a rolling filename.
/// </summary>
public class FileRenderer(Func<string> getFileName, LogFileMode logFileMode, TimeProvider timeProvider)
	: StreamRendererBase
{
	private static readonly TimeSpan FileExistsCheckInterval = TimeSpan.FromSeconds(2);

	private string? openFileName;
	private StreamWriter? streamWriter;
	private FileSystemWatcher? fileWatcher;
	private volatile bool fileDeleted;
	private long lastFileExistsCheckTick = long.MinValue;

	public FileRenderer(string fileName)
		: this(() => fileName, LogFileMode.Append, TimeProvider.System)
	{
	}

	public FileRenderer(string fileName, LogFileMode logFileMode)
		: this(() => fileName, logFileMode, TimeProvider.System)
	{
	}

	public FileRenderer(Func<string> getFileName)
		: this(getFileName, LogFileMode.Append, TimeProvider.System)
	{
	}

	public FileRenderer(Func<string> getFileName, LogFileMode logFileMode)
		: this(getFileName, logFileMode, TimeProvider.System)
	{
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);

		if (disposing)
		{
			fileWatcher?.Dispose();
			fileWatcher = null;

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

		if (streamWriter != null && (openFileName != fileName || fileDeleted || IsFileExistsCheckDue() && !File.Exists(openFileName)))
		{
			await streamWriter.FlushAsync().ConfigureAwait(false);
			await streamWriter.DisposeAsync().ConfigureAwait(false);
			streamWriter = null;

			fileWatcher?.Dispose();
			fileWatcher = null;
			fileDeleted = false;
		}

		openFileName = fileName;

		if (streamWriter == null)
		{
			streamWriter = new StreamWriter(LogFile.OpenFile(fileName, logFileMode));

			lastFileExistsCheckTick = timeProvider.GetTimestamp();

			TrySetupFileWatcher(fileName);
		}

		return streamWriter;
	}

	private bool IsFileExistsCheckDue()
	{
		var now = timeProvider.GetTimestamp();
		var elapsedTime = timeProvider.GetElapsedTime(lastFileExistsCheckTick, now);

		if (elapsedTime > FileExistsCheckInterval)
		{
			lastFileExistsCheckTick = now;

			return true;
		}

		return false;
	}

	private void TrySetupFileWatcher(string fileName)
	{
		try
		{
			var fullPath = Path.GetFullPath(fileName);
			var directory = Path.GetDirectoryName(fullPath);

			if (directory == null)
			{
				return;
			}

			fileWatcher = new FileSystemWatcher(directory, Path.GetFileName(fullPath))
			{
				NotifyFilter = NotifyFilters.FileName,
				EnableRaisingEvents = true
			};

			// FSW handles renames immediately (e.g. log rotation tools that rename-then-delete).
			// It does NOT reliably fire Deleted while our handle is open on Windows — the OS defers
			// the directory-entry removal until the last handle closes — so the throttled File.Exists
			// check in GetStreamWriterAsync covers plain deletions.
			fileWatcher.Deleted += (_, _) =>
			{
				fileDeleted = true;
			};

			fileWatcher.Renamed += (_, e) =>
			{
				if (string.Equals(e.OldFullPath, fullPath, StringComparison.OrdinalIgnoreCase))
				{
					fileDeleted = true;
				}
			};
		}
		catch
		{
			// Non-fatal: the throttled File.Exists check will still handle deletion detection.
			fileWatcher = null;
		}
	}
}
