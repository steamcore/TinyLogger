using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SystemConsole = System.Console;

namespace TinyLogger.Console
{
	/// <summary>
	/// Renders log messages in color to the console.
	/// </summary>
	public class ConsoleRenderer : ILogRenderer
	{
		private readonly Action<TokenizedMessage> renderMessage;

		public ConsoleRenderer()
			: this(new DefaultConsoleTheme())
		{
		}

		public ConsoleRenderer(IConsoleTheme theme)
		{
			renderMessage = SelectRenderMethod();

			Action<TokenizedMessage> SelectRenderMethod()
			{
				// Honor no-color.org
				if (Environment.GetEnvironmentVariable("NO_COLOR") != null)
				{
					return RenderPlainText;
				}

				// Try to enable VT-processing on Windows
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					var handle = NativeMethods.GetStdHandle(NativeMethods.STD_OUTPUT_HANDLE);

					// Fallback to slow classic colors if console mode can't be accessed
					if (!NativeMethods.GetConsoleMode(handle, out var consoleMode))
					{
						return message => RenderWithClassicColors(theme, message);
					}

					if ((consoleMode & NativeMethods.ENABLE_VIRTUAL_TERMINAL_PROCESSING) == 0)
					{
						NativeMethods.SetConsoleMode(handle, consoleMode | NativeMethods.ENABLE_VIRTUAL_TERMINAL_PROCESSING);
					}
				}

				return message => RenderWithAnsiColors(theme, message);
			}
		}

		public Task Render(Func<TokenizedMessage> message)
		{
			renderMessage(message());
			return Task.CompletedTask;
		}

		private static void RenderPlainText(TokenizedMessage message)
		{
			var sb = new StringBuilder(128);

			foreach (var token in message.Message)
			{
				sb.Append(token.ToString());
			}

			SystemConsole.Write(sb.ToString());
		}

		private static void RenderWithAnsiColors(IConsoleTheme theme, TokenizedMessage message)
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
					case (ConsoleColor foreground, ConsoleColor background):
						sb.Append("\x1b[");
						sb.Append(GetForeground(foreground));
						sb.Append(';');
						sb.Append(GetBackground(background));
						sb.Append('m');
						sb.Append(token.ToString());
						sb.Append(Reset);
						break;

					case (ConsoleColor foreground, _):
						sb.Append("\x1b[");
						sb.Append(GetForeground(foreground));
						sb.Append('m');
						sb.Append(token.ToString());
						sb.Append(Reset);
						break;

					case (_, ConsoleColor background):
						sb.Append("\x1b[");
						sb.Append(GetBackground(background));
						sb.Append('m');
						sb.Append(token.ToString());
						sb.Append(Reset);
						break;

					default:
						sb.Append(token.ToString());
						break;
				}
			}

			SystemConsole.Write(sb.ToString());

			static string GetForeground(ConsoleColor color)
			{
				return color switch
				{
					ConsoleColor.Black => "30",
					ConsoleColor.DarkRed => "31",
					ConsoleColor.DarkGreen => "32",
					ConsoleColor.DarkYellow => "33",
					ConsoleColor.DarkBlue => "34",
					ConsoleColor.DarkMagenta => "35",
					ConsoleColor.DarkCyan => "36",
					ConsoleColor.Gray => "37",

					ConsoleColor.DarkGray => "90",
					ConsoleColor.Red => "91",
					ConsoleColor.Green => "92",
					ConsoleColor.Yellow => "93",
					ConsoleColor.Blue => "94",
					ConsoleColor.Magenta => "95",
					ConsoleColor.Cyan => "96",
					ConsoleColor.White => "97",

					_ => "39"
				};
			}

			static string GetBackground(ConsoleColor color)
			{
				return color switch
				{
					ConsoleColor.Black => "40",
					ConsoleColor.DarkRed => "41",
					ConsoleColor.DarkGreen => "42",
					ConsoleColor.DarkYellow => "43",
					ConsoleColor.DarkBlue => "44",
					ConsoleColor.DarkMagenta => "45",
					ConsoleColor.DarkCyan => "46",
					ConsoleColor.Gray => "47",

					ConsoleColor.DarkGray => "100",
					ConsoleColor.Red => "101",
					ConsoleColor.Green => "102",
					ConsoleColor.Yellow => "103",
					ConsoleColor.Blue => "104",
					ConsoleColor.Magenta => "105",
					ConsoleColor.Cyan => "106",
					ConsoleColor.White => "107",

					_ => "49"
				};
			}
		}

		private static void RenderWithClassicColors(IConsoleTheme theme, TokenizedMessage message)
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

		private static class NativeMethods
		{
			public const int STD_OUTPUT_HANDLE = -11;
			public const int STD_ERROR_HANDLE = -12;

			public const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

			[DllImport("kernel32.dll")]
			public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

			[DllImport("kernel32.dll")]
			public static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

			[DllImport("kernel32.dll", SetLastError = true)]
			public static extern IntPtr GetStdHandle(int nStdHandle);
		}
	}
}
