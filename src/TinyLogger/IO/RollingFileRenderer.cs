using System.Text;

namespace TinyLogger.IO;

/// <summary>
/// Render messages in plain text to a file with a rolling filename.
/// </summary>
public class RollingFileRenderer(Func<string> getFileName, LogFileMode logFileMode)
	: ILogRenderer, IDisposable
{
	private readonly Func<string> getFileName = getFileName;
	private readonly LogFileMode logFileMode = logFileMode;

	private bool disposed;
	private string? openFileName;
	private StreamWriter? streamWriter;

	public RollingFileRenderer(Func<string> getFileName)
		: this(getFileName, LogFileMode.Append)
	{
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposed)
		{
			return;
		}

		if (disposing)
		{
			streamWriter?.Dispose();
		}

		disposed = true;
	}

	public Task FlushAsync()
	{
		return streamWriter?.FlushAsync() ?? Task.CompletedTask;
	}

	public async Task RenderAsync(TokenizedMessage message)
	{
		if (disposed || message is null)
		{
			return;
		}

		var fileName = getFileName();

		if (openFileName != fileName || streamWriter is null)
		{
			if (streamWriter != null)
			{
				await streamWriter.FlushAsync();
#if NET
				await streamWriter.DisposeAsync();
#else
				streamWriter.Dispose();
#endif
			}

			streamWriter = new StreamWriter(LogFile.OpenFile(fileName, logFileMode));
			openFileName = fileName;
		}

		using var sb = Pooling.RentStringBuilder();

		for (var i = 0; i < message.MessageTokens.Count; i++)
		{
			var token = message.MessageTokens[i];

			token.Write(sb.Value);
		}

		await sb.Value.WriteToStreamWriterAsync(streamWriter).ConfigureAwait(false);
	}
}
