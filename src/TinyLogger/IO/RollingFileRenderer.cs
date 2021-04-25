using System;
using System.IO;
using System.Threading.Tasks;

namespace TinyLogger.IO
{
	/// <summary>
	/// Render messages in plain text to a file with a rolling filename.
	/// </summary>
	public class RollingFileRenderer : ILogRenderer, IDisposable
	{
		private readonly Func<string> getFileName;
		private readonly FileMode fileMode;

		private bool disposed;
		private string? openFileName;
		private StreamWriter? streamWriter;

		public RollingFileRenderer(Func<string> getFileName)
			: this(getFileName, FileMode.Append)
		{
		}

		public RollingFileRenderer(Func<string> getFileName, FileMode fileMode)
		{
			this.getFileName = getFileName;
			this.fileMode = fileMode;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;

			if (disposing)
			{
				streamWriter?.Dispose();
			}

			disposed = true;
		}

		public async Task Render(TokenizedMessage message)
		{
			if (disposed)
				return;

			var fileName = getFileName();

			if (openFileName != fileName || streamWriter is null)
			{
				streamWriter?.Dispose();
				streamWriter = new StreamWriter(File.Open(fileName, fileMode, FileAccess.Write, FileShare.Read));
				openFileName = fileName;
			}

			foreach (var token in message.MessageTokens)
			{
				await streamWriter.WriteAsync(token.ToString()).ConfigureAwait(false);
			}
		}
	}
}
