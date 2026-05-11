using System.Drawing;
using Microsoft.Extensions.Logging;

namespace TinyLogger.Themes.TrueColorTheme;

public interface ITrueColorTheme
{
	(Color? foreground, Color? background) GetColors(object? value, LogLevel logLevel);
}
