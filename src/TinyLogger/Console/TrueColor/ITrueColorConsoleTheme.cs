using System.Drawing;
using Microsoft.Extensions.Logging;

namespace TinyLogger.Console.TrueColor;

public interface ITrueColorConsoleTheme
{
	(Color? foreground, Color? background) GetColors(object? value, LogLevel logLevel);
}
