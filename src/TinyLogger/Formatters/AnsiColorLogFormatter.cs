using System.Text;
using TinyLogger.Themes.AnsiColorTheme;

namespace TinyLogger.Formatters;

/// <summary>
/// Formats log messages with ANSI escape codes for coloring based on the provided <see cref="IAnsiColorTheme"/>.
/// </summary>
/// <param name="theme">The console color theme to use for determining the colors of log message tokens.</param>
public class AnsiColorLogFormatter(IAnsiColorTheme theme) : ILogFormatter
{
	public AnsiColorLogFormatter()
		: this(new DefaultAnsiColorTheme())
	{
	}

	public void Format(TokenizedMessage message, StringBuilder stringBuilder)
	{
		if (message is null || stringBuilder is null)
		{
			return;
		}

		const string Reset = "\x1b[0m";

		for (var i = 0; i < message.MessageTokens.Count; i++)
		{
			var token = message.MessageTokens[i];

			if (token is LiteralToken || !token.TryGetValue(out var value))
			{
				token.Write(stringBuilder);
				continue;
			}

			switch (theme.GetColors(value, message.LogLevel))
			{
				case (ConsoleColor foreground, ConsoleColor background):
					AppendForeground(stringBuilder, foreground);
					AppendBackground(stringBuilder, background);
					token.Write(stringBuilder);
					stringBuilder.Append(Reset);
					break;

				case (ConsoleColor foreground, _):
					AppendForeground(stringBuilder, foreground);
					token.Write(stringBuilder);
					stringBuilder.Append(Reset);
					break;

				case (_, ConsoleColor background):
					AppendBackground(stringBuilder, background);
					token.Write(stringBuilder);
					stringBuilder.Append(Reset);
					break;

				default:
					token.Write(stringBuilder);
					break;
			}
		}
	}

	private static void AppendForeground(StringBuilder sb, ConsoleColor color)
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

	private static void AppendBackground(StringBuilder sb, ConsoleColor color)
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
