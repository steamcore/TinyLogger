using System.Text;

namespace TinyLogger.Console;

/// <summary>
/// Renders log messages to the console in plain text.
/// </summary>
public class PlainTextConsoleRenderer : ILogRenderer
{
	public Task Flush()
	{
		return Task.CompletedTask;
	}

	public Task Render(TokenizedMessage message)
	{
		using var sb = Pooling.RentStringBuilder();
		using var messageTokens = message.RentMessageTokenList();

		foreach (var token in messageTokens.Value)
		{
			token.Write(sb.Value);
		}

		sb.Value.WriteToConsole();

		return Task.CompletedTask;
	}
}
