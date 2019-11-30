using System;
using System.Threading.Tasks;
using SystemConsole = System.Console;

namespace TinyLogger.Console
{
	/// <summary>
	/// Renders log messages in color to the console.
	/// </summary>
	public class ConsoleRenderer : ILogRenderer
	{
		private readonly IConsoleTheme theme;

		public ConsoleRenderer()
			: this(new DefaultConsoleTheme())
		{
		}

		public ConsoleRenderer(IConsoleTheme theme)
		{
			this.theme = theme;
		}

		public Task Render(Func<TokenizedMessage> message)
		{
			RenderMessage(theme, message());
			return Task.CompletedTask;
		}

		private static void RenderMessage(IConsoleTheme theme, TokenizedMessage message)
		{
			foreach (var token in message.Message)
			{
				RenderToken(theme, message, token);
			}
		}

		private static void RenderToken(IConsoleTheme theme, TokenizedMessage message, MessageToken token)
		{
			ConsoleColor? foreground = null;
			ConsoleColor? background = null;

			if (token.Type == MessageTokenType.ObjectToken)
			{
				(foreground, background) = theme.GetColors(token.Value, message.LogLevel);
			}

			RenderToken(token.ToString(), foreground, background);
		}

		private static void RenderToken(string message, ConsoleColor? foreground = null, ConsoleColor? background = null)
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
