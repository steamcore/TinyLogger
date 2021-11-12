using System;
using System.Diagnostics.CodeAnalysis;
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
		private readonly LogFileMode logFileMode;

		private bool disposed;
		private string? openFileName;
		private StreamWriter? streamWriter;

		public RollingFileRenderer(Func<string> getFileName)
			: this(getFileName, LogFileMode.Append)
		{
		}

		public RollingFileRenderer(Func<string> getFileName, LogFileMode logFileMode)
		{
			this.getFileName = getFileName;
			this.logFileMode = logFileMode;
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

		public Task Flush()
		{
			return streamWriter?.FlushAsync() ?? Task.CompletedTask;
		}

		[SuppressMessage("Performance", "CA1849: Call async methods when in an async method", Justification = "False positive")]
		public async Task Render(TokenizedMessage message)
		{
			if (disposed)
				return;

			var fileName = getFileName();

			if (openFileName != fileName || streamWriter is null)
			{
				if (streamWriter != null)
				{
					await streamWriter.FlushAsync();
					streamWriter.Dispose();
				}

				streamWriter = new StreamWriter(LogFile.OpenFile(fileName, logFileMode));
				openFileName = fileName;
			}

			foreach (var token in message.MessageTokens)
			{
				await streamWriter.WriteAsync(token.ToString()).ConfigureAwait(false);
			}
		}
	}
}
