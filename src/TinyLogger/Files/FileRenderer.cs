using System;
using System.IO;
using System.Threading.Tasks;

namespace TinyLogger.Files
{
	/// <summary>
	/// Renders log messages in plain text to a file.
	/// </summary>
	public class FileRenderer : ILogRenderer, IDisposable
	{
		private readonly Func<string> getFileName;

		private bool disposed;
		private string? openFileName;
		private StreamWriter? streamWriter;

		public FileRenderer(Func<string> getFileName)
		{
			this.getFileName = getFileName;
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

			if (openFileName != fileName || streamWriter == null)
			{
				streamWriter?.Dispose();
				streamWriter = new StreamWriter(File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.Read));
				openFileName = fileName;
			}

			foreach (var token in message.MessageTokens)
			{
				await streamWriter.WriteAsync(token.ToString()).ConfigureAwait(false);
			}
		}
	}
}
