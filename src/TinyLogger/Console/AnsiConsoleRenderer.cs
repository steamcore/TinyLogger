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
		using var messageTokens = message.RentMessageTokenList();

		for (var i = 0; i < messageTokens.Value.Count; i++)
		{
			var token = messageTokens.Value[i];

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
