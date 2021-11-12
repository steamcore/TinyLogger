namespace TinyLogger.IO
{
	internal static class LogFile
	{
		internal static Stream OpenFile(string fileName, LogFileMode logFileMode)
		{
			if (logFileMode == LogFileMode.Truncate)
			{
				var fs = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
				fs.SetLength(0);
				return fs;
			}

			return File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.Read);
		}
	}
}
