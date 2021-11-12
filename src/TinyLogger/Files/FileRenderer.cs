using TinyLogger.IO;

namespace TinyLogger.Files
{
	/// <summary>
	/// Renders log messages in plain text to a file.
	/// </summary>
	[Obsolete("Use FileRenderer or RollingFileRenderer in TinyLogger.IO")]
	public class FileRenderer : ILogRenderer, IDisposable
	{
		private readonly RollingFileRenderer innerRenderer;

		private bool disposed;

		public FileRenderer(Func<string> getFileName)
		{
			innerRenderer = new RollingFileRenderer(getFileName);
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
				innerRenderer.Dispose();
			}

			disposed = true;
		}

		public Task Flush()
		{
			return innerRenderer.Flush();
		}

		public Task Render(TokenizedMessage message)
		{
			return innerRenderer.Render(message);
		}
	}
}
