using System.Drawing;
using System.Text;

namespace TinyLogger.Console.TrueColor;

public class TrueColorConsoleRenderer(ITrueColorConsoleTheme theme)
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

	private static void Render(ITrueColorConsoleTheme theme, TokenizedMessage message)
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
				case (Color foreground, Color background):
					AppendForeground(sb.Value, foreground);
					AppendBackground(sb.Value, background);
					token.Write(sb.Value);
					sb.Value.Append(Reset);
					break;

				case (Color foreground, _):
					AppendForeground(sb.Value, foreground);
					token.Write(sb.Value);
					sb.Value.Append(Reset);
					break;

				case (_, Color background):
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
