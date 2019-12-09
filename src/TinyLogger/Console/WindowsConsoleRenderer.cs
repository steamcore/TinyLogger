using System;
using System.Threading.Tasks;
using SystemConsole = System.Console;

namespace TinyLogger.Console
{
	/// <summary>
	/// Renders log messages to the console in color by using the legacy Windows API.
	/// </summary>
	public class WindowsConsoleRenderer : ILogRenderer
	{
		private readonly IConsoleTheme theme;

		public WindowsConsoleRenderer(IConsoleTheme theme)
		{
			this.theme = theme;
		}

		public Task Render(Func<TokenizedMessage> message)
		{
			Render(theme, message());
			return Task.CompletedTask;
		}

		private static void Render(IConsoleTheme theme, TokenizedMessage message)
		{
			foreach (var token in message.Message)
			{
				ConsoleColor? foreground = null;
				ConsoleColor? background = null;

				if (token.Type == MessageTokenType.ObjectToken)
				{
					(foreground, background) = theme.GetColors(token.Value, message.LogLevel);
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
}
