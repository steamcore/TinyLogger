using System;
using System.Text;
using System.Threading.Tasks;
using SystemConsole = System.Console;

namespace TinyLogger.Console
{
	/// <summary>
	/// Renders log messages to the console in color by using ANSI control codes.
	/// </summary>
	public class AnsiConsoleRenderer : ILogRenderer
	{
		private readonly IConsoleTheme theme;

		public AnsiConsoleRenderer(IConsoleTheme theme)
		{
			this.theme = theme;
		}

		public Task Render(TokenizedMessage message)
		{
			Render(theme, message);
			return Task.CompletedTask;
		}

		private static void Render(IConsoleTheme theme, TokenizedMessage message)
		{
			const string Reset = "\x1b[0m";

			var sb = new StringBuilder(128);

			foreach (var token in message.MessageTokens)
			{
				if (token.Type != MessageTokenType.ObjectToken)
				{
					sb.Append(token.ToString());
					continue;
				}

				switch (theme.GetColors(token.Value, message.LogLevel))
				{
					case (ConsoleColor foreground, ConsoleColor background):
						AppendForeground(sb, foreground);
						AppendBackground(sb, background);
						sb.Append(token.ToString());
						sb.Append(Reset);
						break;

					case (ConsoleColor foreground, _):
						AppendForeground(sb, foreground);
						sb.Append(token.ToString());
						sb.Append(Reset);
						break;

					case (_, ConsoleColor background):
						AppendBackground(sb, background);
						sb.Append(token.ToString());
						sb.Append(Reset);
						break;

					default:
						sb.Append(token.ToString());
						break;
				}
			}

			SystemConsole.Write(sb.ToString());

			static void AppendForeground(StringBuilder sb, ConsoleColor color)
			{
				sb.Append(color switch
				{
					ConsoleColor.Black => "\x1b[30m",
					ConsoleColor.DarkRed => "\x1b[31m",
					ConsoleColor.DarkGreen => "\x1b[32m",
					ConsoleColor.DarkYellow => "\x1b[33m",
					ConsoleColor.DarkBlue => "\x1b[34m",
					ConsoleColor.DarkMagenta => "\x1b[35m",
					ConsoleColor.DarkCyan => "\x1b[36m",
					ConsoleColor.Gray => "\x1b[37m",

					ConsoleColor.DarkGray => "\x1b[90m",
					ConsoleColor.Red => "\x1b[91m",
					ConsoleColor.Green => "\x1b[92m",
					ConsoleColor.Yellow => "\x1b[93m",
					ConsoleColor.Blue => "\x1b[94m",
					ConsoleColor.Magenta => "\x1b[95m",
					ConsoleColor.Cyan => "\x1b[96m",
					ConsoleColor.White => "\x1b[97m",

					_ => "\x1b[39m"
				});
			}

			static void AppendBackground(StringBuilder sb, ConsoleColor color)
			{
				sb.Append(color switch
				{
					ConsoleColor.Black => "\x1b[40m",
					ConsoleColor.DarkRed => "\x1b[41m",
					ConsoleColor.DarkGreen => "\x1b[42m",
					ConsoleColor.DarkYellow => "\x1b[43m",
					ConsoleColor.DarkBlue => "\x1b[44m",
					ConsoleColor.DarkMagenta => "\x1b[45m",
					ConsoleColor.DarkCyan => "\x1b[46m",
					ConsoleColor.Gray => "\x1b[47m",

					ConsoleColor.DarkGray => "\x1b[100m",
					ConsoleColor.Red => "\x1b[101m",
					ConsoleColor.Green => "\x1b[102m",
					ConsoleColor.Yellow => "\x1b[103m",
					ConsoleColor.Blue => "\x1b[104m",
					ConsoleColor.Magenta => "\x1b[105m",
					ConsoleColor.Cyan => "\x1b[106m",
					ConsoleColor.White => "\x1b[107m",

					_ => "\x1b[49m"
				});
			}
		}
	}
}
