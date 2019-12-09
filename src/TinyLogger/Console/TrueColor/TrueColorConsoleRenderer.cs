using System;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using SystemConsole = System.Console;

namespace TinyLogger.Console.TrueColor
{
	public class TrueColorConsoleRenderer : ILogRenderer
	{
		private readonly ITrueColorConsoleTheme theme;

		public TrueColorConsoleRenderer(ITrueColorConsoleTheme theme)
		{
			this.theme = theme;
		}

		public Task Render(Func<TokenizedMessage> message)
		{
			Render(theme, message());
			return Task.CompletedTask;
		}

		private static void Render(ITrueColorConsoleTheme theme, TokenizedMessage message)
		{
			const string Reset = "\x1b[0m";

			var sb = new StringBuilder(128);

			foreach (var token in message.Message)
			{
				if (token.Type != MessageTokenType.ObjectToken)
				{
					sb.Append(token.ToString());
					continue;
				}

				switch (theme.GetColors(token.Value, message.LogLevel))
				{
					case (Color foreground, Color background):
						AppendForeground(sb, foreground);
						AppendBackground(sb, background);
						sb.Append(token.ToString());
						sb.Append(Reset);
						break;

					case (Color foreground, _):
						AppendForeground(sb, foreground);
						sb.Append(token.ToString());
						sb.Append(Reset);
						break;

					case (_, Color background):
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
}
