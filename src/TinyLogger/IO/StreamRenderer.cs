namespace TinyLogger.IO
{
	/// <summary>
	/// Renders log messages in plain text to a stream.
	/// </summary>
	public class StreamRenderer : ILogRenderer, IDisposable
	{
		private readonly Func<StreamWriter> createStreamWriter;
		private readonly object streamWriterLock = new object();

		private bool disposed;
		private StreamWriter? streamWriter;

		public StreamRenderer(Stream stream)
			: this(new Func<StreamWriter>(() => new StreamWriter(stream)))
		{
		}

		public StreamRenderer(StreamWriter streamWriter)
			: this(new Func<StreamWriter>(() => streamWriter))
		{
		}

		public StreamRenderer(Func<StreamWriter> createStreamWriter)
		{
			this.createStreamWriter = createStreamWriter ?? throw new ArgumentNullException(nameof(createStreamWriter));
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

		public async Task Render(TokenizedMessage message)
		{
			if (disposed)
				return;

			foreach (var token in message.MessageTokens)
			{
				await GetStreamWriter().WriteAsync(token.ToString()).ConfigureAwait(false);
			}
		}

		private StreamWriter GetStreamWriter()
		{
			if (streamWriter != null)
			{
				return streamWriter;
			}

			lock (streamWriterLock)
			{
				return streamWriter ??= createStreamWriter();
			}
		}
	}
}
