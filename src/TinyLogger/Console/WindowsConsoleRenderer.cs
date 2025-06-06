using SystemConsole = System.Console;

namespace TinyLogger.Console;

/// <summary>
/// Renders log messages to the console in color by using the legacy Windows API.
/// </summary>
public class WindowsConsoleRenderer(IConsoleTheme theme)
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

		Render(theme, message);

		return Task.CompletedTask;
	}

	private static void Render(IConsoleTheme theme, TokenizedMessage message)
	{
		foreach (var token in message.MessageTokens)
		{
			ConsoleColor? foreground = null;
			ConsoleColor? background = null;

			if (token.TryGetValue(out var value))
			{
				(foreground, background) = theme.GetColors(value, message.LogLevel);
			}

			RenderToken(token.ToString(), foreground, background);
		}

		static void RenderToken(string message, ConsoleColor? foreground = null, ConsoleColor? background = null)
		{
			try
			{
				if (background != null)
				{
					SystemConsole.BackgroundColor = background.Value;
				}

				if (foreground != null)
				{
					SystemConsole.ForegroundColor = foreground.Value;
				}

				SystemConsole.Write(message);
			}
			finally
			{
				if (background != null || foreground != null)
				{
					SystemConsole.ResetColor();
				}
			}
		}
	}
}
