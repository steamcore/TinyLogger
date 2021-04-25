using System;
using System.IO;

namespace TinyLogger.IO
{
	/// <summary>
	/// Renders log messages in plain text to a file.
	/// </summary>
	public class FileRenderer : StreamRenderer
	{
		public FileRenderer(string fileName)
			: this(fileName, FileMode.Append)
		{
		}

		public FileRenderer(string fileName, FileMode fileMode)
			: base(new Func<StreamWriter>(() => new StreamWriter(File.Open(fileName, fileMode, FileAccess.Write, FileShare.Read))))
		{
		}
	}
}
