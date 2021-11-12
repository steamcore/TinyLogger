using Microsoft.Extensions.Logging;

namespace TinyLogger.Console;

public class DefaultConsoleTheme : IConsoleTheme
{
	public virtual (ConsoleColor? foreground, ConsoleColor? background) GetColors(object? value, LogLevel logLevel)
	{
		return (value, logLevel) switch
		{
			(bool b, _) => (b ? ConsoleColor.Green : ConsoleColor.Red, null),

			(byte _, _) => (ConsoleColor.Magenta, null),
			(sbyte _, _) => (ConsoleColor.Magenta, null),
			(short _, _) => (ConsoleColor.Magenta, null),
			(ushort _, _) => (ConsoleColor.Magenta, null),
			(int _, _) => (ConsoleColor.Magenta, null),
			(uint _, _) => (ConsoleColor.Magenta, null),
			(long _, _) => (ConsoleColor.Magenta, null),
			(ulong _, _) => (ConsoleColor.Magenta, null),
			(float _, _) => (ConsoleColor.Magenta, null),
			(double _, _) => (ConsoleColor.Magenta, null),
			(decimal _, _) => (ConsoleColor.Magenta, null),

			(char _, _) _ => (ConsoleColor.Yellow, null),
			(string _, _) _ => (ConsoleColor.Yellow, null),

			(DateTime _, _) _ => (ConsoleColor.Cyan, null),
			(DateTimeOffset _, _) _ => (ConsoleColor.Cyan, null),
			(TimeSpan _, _) _ => (ConsoleColor.Cyan, null),

			(Guid _, _) _ => (ConsoleColor.DarkMagenta, null),
			(Uri _, _) _ => (ConsoleColor.Blue, null),
			(Version _, _) => (ConsoleColor.DarkCyan, null),

			(Exception _, LogLevel.Warning) => (ConsoleColor.Yellow, null),
			(Exception _, LogLevel.Error) => (ConsoleColor.Red, null),
			(Exception _, LogLevel.Critical) => (ConsoleColor.Red, null),

			(EventId _, _) => (ConsoleColor.DarkGray, null),

			(LogLevel level, _) when level == LogLevel.Trace => (ConsoleColor.DarkGray, null),
			(LogLevel level, _) when level == LogLevel.Debug => (ConsoleColor.DarkBlue, null),
			(LogLevel level, _) when level == LogLevel.Information => (ConsoleColor.DarkGreen, null),
			(LogLevel level, _) when level == LogLevel.Warning => (ConsoleColor.Yellow, null),
			(LogLevel level, _) when level == LogLevel.Error => (ConsoleColor.Red, null),
			(LogLevel level, _) when level == LogLevel.Critical => (ConsoleColor.White, ConsoleColor.DarkRed),

			(_, _) => (ConsoleColor.White, null)
		};
	}
}
