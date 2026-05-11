using System.Text;
using TinyLogger.Formatters;

namespace TinyLogger.Console;

/// <summary>
/// Renders log messages to the console using the supplied log formatter.
/// </summary>
public class ConsoleRenderer(ILogFormatter logFormatter)
	: ILogRenderer
{
	public Task FlushAsync()
	{
		return Task.CompletedTask;
	}

	public Task RenderAsync(TokenizedMessage message)
	{
		if (message is null)
		{
			return Task.CompletedTask;
		}

		using var sb = Pooling.RentStringBuilder();

		logFormatter.Format(message, sb.Value);

		sb.Value.WriteToConsole();

		return Task.CompletedTask;
	}
}
