using Microsoft.Extensions.Logging;

namespace TinyLogger.Themes.AnsiColorTheme;

public interface IAnsiColorTheme
{
	(ConsoleColor? foreground, ConsoleColor? background) GetColors(object? value, LogLevel logLevel);
}
