using TinyLogger.Console;
using TinyLogger.Formatters;
using TinyLogger.IO;
using TinyLogger.Themes.AnsiColorTheme;
using TinyLogger.Themes.TrueColorTheme;

namespace TinyLogger;

internal enum ColorMode
{
	Ansi,
	Windows,
	PlainText
}

public static class TinyLoggerOptionsExtensions
{
	extension(TinyLoggerOptions options)
	{
		/// <summary>
		/// Render messages to the console using the default color theme.
		/// </summary>
		public TinyLoggerOptions AddConsole()
		{
			return AddConsole(options, new DefaultAnsiColorTheme());
		}

		/// <summary>
		/// Render messages to the console using a specific color theme.
		/// </summary>
		public TinyLoggerOptions AddConsole(IAnsiColorTheme theme)
		{
			ArgumentNullException.ThrowIfNull(options);

			var mode = DetectColorMode();

			if (mode == ColorMode.Windows)
			{
				options.Renderers.Add(new WindowsConsoleRenderer(theme));
				return options;
			}

			ILogFormatter formatter = mode == ColorMode.Ansi
				? new AnsiColorLogFormatter(theme)
				: PlainTextLogFormatter.Instance;

			options.Renderers.Add(new ConsoleRenderer(formatter));

			return options;
		}

		/// <summary>
		/// Render messages to the console.
		/// </summary>
		public TinyLoggerOptions AddConsole(ILogFormatter formatter)
		{
			ArgumentNullException.ThrowIfNull(options);
			ArgumentNullException.ThrowIfNull(formatter);

			options.Renderers.Add(new ConsoleRenderer(formatter));

			return options;
		}

		/// <summary>
		/// Render messages to the console using plain text.
		/// </summary>
		public TinyLoggerOptions AddPlainTextConsole()
		{
			return AddConsole(options, PlainTextLogFormatter.Instance);
		}

		/// <summary>
		/// Render messages to the console using the default RGB color theme.
		/// </summary>
		public TinyLoggerOptions AddTrueColorConsole()
		{
			return AddTrueColorConsole(options, new DefaultTrueColorTheme());
		}

		/// <summary>
		/// Render messages to the console using a specific RGB color theme.
		/// </summary>
		public TinyLoggerOptions AddTrueColorConsole(ITrueColorTheme theme)
		{
			ILogFormatter formatter = DetectColorMode() == ColorMode.Ansi
				? new TrueColorLogFormatter(theme)
				: PlainTextLogFormatter.Instance;

			return AddConsole(options, formatter);
		}

		/// <summary>
		/// Render messages in plain text to a file.
		/// </summary>
		/// <param name="fileName">The file name to write to.</param>
		/// <param name="logFileMode">Append or truncate log file</param>
		public TinyLoggerOptions AddFile(string fileName, LogFileMode logFileMode = LogFileMode.Append)
		{
			return AddFile(options, fileName, PlainTextLogFormatter.Instance, logFileMode);
		}

		/// <summary>
		/// Render messages to a file.
		/// </summary>
		/// <param name="fileName">The file name to write to.</param>
		/// <param name="formatter">The formatter to use when rendering log messages.</param>
		/// <param name="logFileMode">Append or truncate log file</param>
		public TinyLoggerOptions AddFile(string fileName, ILogFormatter formatter, LogFileMode logFileMode = LogFileMode.Append)
		{
			ArgumentNullException.ThrowIfNull(options);

			options.Renderers.Add(new FileRenderer(formatter, fileName, logFileMode));

			return options;
		}

		/// <summary>
		/// Render messages in plain text to a file with a rolling filename.
		/// </summary>
		/// <param name="getFileName">Retrieve a filename to write to, if the value changes a new file with that file name will be created.</param>
		/// <param name="logFileMode">Append or truncate log file</param>
		public TinyLoggerOptions AddRollingFile(Func<string> getFileName, LogFileMode logFileMode = LogFileMode.Append)
		{
			ArgumentNullException.ThrowIfNull(options);

			options.Renderers.Add(new FileRenderer(PlainTextLogFormatter.Instance, getFileName, logFileMode));

			return options;
		}

		/// <summary>
		/// Render messages in to a file with a rolling filename.
		/// </summary>
		/// <param name="getFileName">Retrieve a filename to write to, if the value changes a new file with that file name will be created.</param>
		/// <param name="formatter">The formatter to use when rendering log messages.</param>
		/// <param name="logFileMode">Append or truncate log file</param>
		public TinyLoggerOptions AddRollingFile(Func<string> getFileName, ILogFormatter formatter, LogFileMode logFileMode = LogFileMode.Append)
		{
			ArgumentNullException.ThrowIfNull(options);
			ArgumentNullException.ThrowIfNull(formatter);

			options.Renderers.Add(new FileRenderer(formatter, getFileName, logFileMode));

			return options;
		}

		/// <summary>
		/// Render messages in plain text to a stream.
		/// </summary>
		/// <param name="stream">The Stream to write to.</param>
		public TinyLoggerOptions AddStream(Stream stream)
		{
			return AddStream(options, stream, PlainTextLogFormatter.Instance);
		}

		/// <summary>
		/// Render messages to a stream.
		/// </summary>
		/// <param name="stream">The Stream to write to.</param>
		/// <param name="formatter">The formatter to use when rendering log messages.</param>
		public TinyLoggerOptions AddStream(Stream stream, ILogFormatter formatter)
		{
			ArgumentNullException.ThrowIfNull(options);
			ArgumentNullException.ThrowIfNull(stream);
			ArgumentNullException.ThrowIfNull(formatter);

			options.Renderers.Add(new StreamRenderer(formatter, stream));

			return options;
		}
	}

	private static ColorMode DetectColorMode()
	{
		// These environment variables are commonly used to force color output in various tools and libraries, so we check them to determine if color output should be enabled.
		var forceColor = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ANSI_COLOR"))
			|| !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FORCE_COLOR"))
			|| !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DOTNET_SYSTEM_CONSOLE_ALLOW_ANSI_COLOR_REDIRECTION"));

		// Honor no-color.org
		var noColor = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR"));

		// Enable color output if forced by environment variables or if the output is not redirected (i.e., it's a terminal).
		if (forceColor || !noColor && !System.Console.IsOutputRedirected)
		{
			// Always use ANSI when color is forced since ANSI support can't be reliably detected when console is redirected
			if (forceColor || AnsiSupport.TryEnable())
			{
				return ColorMode.Ansi;
			}
			else
			{
				return ColorMode.Windows;
			}
		}

		return ColorMode.PlainText;
	}
}
