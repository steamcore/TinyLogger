using Microsoft.Extensions.Logging;

namespace TinyLogger.Console
{
	public interface IConsoleTheme
	{
		(ConsoleColor? foreground, ConsoleColor? background) GetColors(object? value, LogLevel logLevel);
	}
}
