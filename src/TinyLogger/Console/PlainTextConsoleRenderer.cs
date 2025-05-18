using System.Text;

namespace TinyLogger.Console;

/// <summary>
/// Renders log messages to the console in plain text.
/// </summary>
public class PlainTextConsoleRenderer : ILogRenderer
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

		for (var i = 0; i < message.MessageTokens.Count; i++)
		{
			var token = message.MessageTokens[i];

			token.Write(sb.Value);
		}

		sb.Value.WriteToConsole();

		return Task.CompletedTask;
	}
}
