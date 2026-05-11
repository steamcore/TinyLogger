using System.Drawing;
using System.Text;
using TinyLogger.Themes.TrueColorTheme;

namespace TinyLogger.Formatters;

/// <summary>
/// Formats log messages with ANSI escape codes for RGB coloring based on the provided <see cref="ITrueColorTheme"/>.
/// </summary>
/// <param name="theme">The true color theme to use for determining the RGB colors of log message tokens.</param>
public class TrueColorLogFormatter(ITrueColorTheme theme) : ILogFormatter
{
	public TrueColorLogFormatter()
		: this(new DefaultTrueColorTheme())
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
				case (Color foreground, Color background):
					AppendForeground(stringBuilder, foreground);
					AppendBackground(stringBuilder, background);
					token.Write(stringBuilder);
					stringBuilder.Append(Reset);
					break;

				case (Color foreground, _):
					AppendForeground(stringBuilder, foreground);
					token.Write(stringBuilder);
					stringBuilder.Append(Reset);
					break;

				case (_, Color background):
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

	private static void AppendForeground(StringBuilder sb, Color color)
	{
		sb.Append("\x1b[38;2;");
		sb.Append(color.R);
		sb.Append(';');
		sb.Append(color.G);
		sb.Append(';');
		sb.Append(color.B);
		sb.Append('m');
	}

	private static void AppendBackground(StringBuilder sb, Color color)
	{
		sb.Append("\x1b[48;2;");
		sb.Append(color.R);
		sb.Append(';');
		sb.Append(color.G);
		sb.Append(';');
		sb.Append(color.B);
		sb.Append('m');
	}
}
