using System.Text;

namespace TinyLogger.Console;

/// <summary>
/// Renders log messages to the console in color by using ANSI control codes.
/// </summary>
public class AnsiConsoleRenderer(IConsoleTheme theme)
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
		const string Reset = "\x1b[0m";

		using var sb = Pooling.RentStringBuilder();

		for (var i = 0; i < message.MessageTokens.Count; i++)
		{
			var token = message.MessageTokens[i];

			if (token is LiteralToken || !token.TryGetValue(out var value))
			{
				token.Write(sb.Value);
				continue;
			}

			switch (theme.GetColors(value, message.LogLevel))
			{
				case (ConsoleColor foreground, ConsoleColor background):
					AppendForeground(sb.Value, foreground);
					AppendBackground(sb.Value, background);
					token.Write(sb.Value);
					sb.Value.Append(Reset);
					break;

				case (ConsoleColor foreground, _):
					AppendForeground(sb.Value, foreground);
					token.Write(sb.Value);
					sb.Value.Append(Reset);
					break;

				case (_, ConsoleColor background):
					AppendBackground(sb.Value, background);
					token.Write(sb.Value);
					sb.Value.Append(Reset);
					break;

				default:
					token.Write(sb.Value);
					break;
			}
		}

		sb.Value.WriteToConsole();

		static void AppendForeground(StringBuilder sb, ConsoleColor color)
		{
			sb.Append(color switch
			{
				ConsoleColor.Black => "\x1b[38;5;0m",
				ConsoleColor.DarkRed => "\x1b[38;5;1m",
				ConsoleColor.DarkGreen => "\x1b[38;5;2m",
				ConsoleColor.DarkYellow => "\x1b[38;5;3m",
				ConsoleColor.DarkBlue => "\x1b[38;5;4m",
				ConsoleColor.DarkMagenta => "\x1b[38;5;5m",
				ConsoleColor.DarkCyan => "\x1b[38;5;6m",
				ConsoleColor.Gray => "\x1b[38;5;7m",

				ConsoleColor.DarkGray => "\x1b[38;5;8m",
				ConsoleColor.Red => "\x1b[38;5;9m",
				ConsoleColor.Green => "\x1b[38;5;10m",
				ConsoleColor.Yellow => "\x1b[38;5;11m",
				ConsoleColor.Blue => "\x1b[38;5;12m",
				ConsoleColor.Magenta => "\x1b[38;5;13m",
				ConsoleColor.Cyan => "\x1b[38;5;14m",
				ConsoleColor.White => "\x1b[38;5;15m",

				_ => "\x1b[39m"
			});
		}

		static void AppendBackground(StringBuilder sb, ConsoleColor color)
		{
			sb.Append(color switch
			{
				ConsoleColor.Black => "\x1b[48;5;0m",
				ConsoleColor.DarkRed => "\x1b[48;5;1m",
				ConsoleColor.DarkGreen => "\x1b[48;5;2m",
				ConsoleColor.DarkYellow => "\x1b[48;5;3m",
				ConsoleColor.DarkBlue => "\x1b[48;5;4m",
				ConsoleColor.DarkMagenta => "\x1b[48;5;5m",
				ConsoleColor.DarkCyan => "\x1b[48;5;6m",
				ConsoleColor.Gray => "\x1b[48;5;7m",

				ConsoleColor.DarkGray => "\x1b[48;5;8m",
				ConsoleColor.Red => "\x1b[48;5;9m",
				ConsoleColor.Green => "\x1b[48;5;10m",
				ConsoleColor.Yellow => "\x1b[48;5;11m",
				ConsoleColor.Blue => "\x1b[48;5;12m",
				ConsoleColor.Magenta => "\x1b[48;5;13m",
				ConsoleColor.Cyan => "\x1b[48;5;14m",
				ConsoleColor.White => "\x1b[48;5;15m",

				_ => "\x1b[49m"
			});
		}
	}
}
