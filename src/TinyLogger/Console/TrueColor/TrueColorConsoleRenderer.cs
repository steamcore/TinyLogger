using System.Drawing;
using System.Text;

namespace TinyLogger.Console.TrueColor;

public class TrueColorConsoleRenderer : ILogRenderer
{
	private readonly ITrueColorConsoleTheme theme;

	public TrueColorConsoleRenderer(ITrueColorConsoleTheme theme)
	{
		this.theme = theme;
	}

	public Task Flush()
	{
		return Task.CompletedTask;
	}

	public Task Render(TokenizedMessage message)
	{
		Render(theme, message);
		return Task.CompletedTask;
	}

	private static void Render(ITrueColorConsoleTheme theme, TokenizedMessage message)
	{
		const string Reset = "\x1b[0m";

		using var sb = Pooling.RentStringBuilder();
		using var messageTokens = message.RentMessageTokenList();

		for (var i = 0; i < messageTokens.Value.Count; i++)
		{
			var token = messageTokens.Value[i];

			if (token.Type != MessageTokenType.ObjectToken)
			{
				sb.Value.Append(token.ToString());
				continue;
			}

			switch (theme.GetColors(token.Value, message.LogLevel))
			{
				case (Color foreground, Color background):
					AppendForeground(sb.Value, foreground);
					AppendBackground(sb.Value, background);
					sb.Value.Append(token.ToString());
					sb.Value.Append(Reset);
					break;

				case (Color foreground, _):
					AppendForeground(sb.Value, foreground);
					sb.Value.Append(token.ToString());
					sb.Value.Append(Reset);
					break;

				case (_, Color background):
					AppendBackground(sb.Value, background);
					sb.Value.Append(token.ToString());
					sb.Value.Append(Reset);
					break;

				default:
					sb.Value.Append(token.ToString());
					break;
			}
		}

		sb.Value.WriteToConsole();

		static void AppendForeground(StringBuilder sb, Color color)
		{
			sb.Append("\x1b[38;2;");
			sb.Append(color.R);
			sb.Append(';');
			sb.Append(color.G);
			sb.Append(';');
			sb.Append(color.B);
			sb.Append('m');
		}

		static void AppendBackground(StringBuilder sb, Color color)
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
}
