using System;
using Microsoft.Extensions.Logging;

namespace TinyLogger.Console
{
	public class DefaultConsoleTheme : IConsoleTheme
	{
		private static readonly ConsoleColor? none = null;

		public virtual (ConsoleColor? foreground, ConsoleColor? background) GetColors(object? value, LogLevel logLevel)
		{
			return (value, logLevel) switch
			{
				(int _, _) => (ConsoleColor.Magenta, none),
				(float _, _) => (ConsoleColor.Magenta, none),
				(double _, _) => (ConsoleColor.Magenta, none),
				(decimal _, _) => (ConsoleColor.Magenta, none),
				(string _, _) _ => (ConsoleColor.Yellow, none),

				(DateTime _, _) _ => (ConsoleColor.Cyan, none),
				(TimeSpan _, _) _ => (ConsoleColor.Cyan, none),
				(Guid _, _) _ => (ConsoleColor.DarkMagenta, none),
				(Uri _, _) _ => (ConsoleColor.Blue, none),

				(Exception _, LogLevel.Warning) => (ConsoleColor.Yellow, none),
				(Exception _, LogLevel.Error) => (ConsoleColor.Red, none),
				(Exception _, LogLevel.Critical) => (ConsoleColor.Red, none),

				(EventId _, _) => (ConsoleColor.DarkGray, none),

				(LogLevel level, _) when level == LogLevel.Trace => (ConsoleColor.DarkGray, none),
				(LogLevel level, _) when level == LogLevel.Debug => (ConsoleColor.DarkBlue, none),
				(LogLevel level, _) when level == LogLevel.Information => (ConsoleColor.DarkGreen, none),
				(LogLevel level, _) when level == LogLevel.Warning => (ConsoleColor.Yellow, none),
				(LogLevel level, _) when level == LogLevel.Error => (ConsoleColor.Red, none),
				(LogLevel level, _) when level == LogLevel.Critical => (ConsoleColor.White, ConsoleColor.DarkRed),

				(_, _) => (ConsoleColor.White, none)
			};
		}
	}
}
