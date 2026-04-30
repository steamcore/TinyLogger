using TinyLogger.Console;
using TinyLogger.Console.TrueColor;
using TinyLogger.IO;

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
			return AddConsole(options, new DefaultConsoleTheme());
		}

		/// <summary>
		/// Render messages to the console using a specific color theme.
		/// </summary>
		public TinyLoggerOptions AddConsole(IConsoleTheme theme)
		{
			ArgumentNullException.ThrowIfNull(options);

			options.Renderers.Add(DetectColorMode() switch
			{
				ColorMode.Ansi => new AnsiConsoleRenderer(theme),
				ColorMode.Windows => new WindowsConsoleRenderer(theme),
				_ => new PlainTextConsoleRenderer()
			});

			return options;
		}

		/// <summary>
		/// Render messages to the console using plain text.
		/// </summary>
		public TinyLoggerOptions AddPlainTextConsole()
		{
			ArgumentNullException.ThrowIfNull(options);

			options.Renderers.Add(new PlainTextConsoleRenderer());

			return options;
		}

		/// <summary>
		/// Render messages to the console using the default RGB color theme.
		/// </summary>
		public TinyLoggerOptions AddTrueColorConsole()
		{
			return AddTrueColorConsole(options, new DefaultTrueColorConsoleTheme());
		}

		/// <summary>
		/// Render messages to the console using a specific RGB color theme.
		/// </summary>
		public TinyLoggerOptions AddTrueColorConsole(ITrueColorConsoleTheme theme)
		{
			ArgumentNullException.ThrowIfNull(options);

			options.Renderers.Add(DetectColorMode() switch
			{
				ColorMode.Ansi => new TrueColorConsoleRenderer(theme),
				_ => new PlainTextConsoleRenderer()
			});

			return options;
		}

		/// <summary>
		/// Render messages in plain text to a file.
		/// </summary>
		/// <param name="fileName">The file name to write to.</param>
		public TinyLoggerOptions AddFile(string fileName)
		{
			return options.AddFile(fileName, LogFileMode.Append);
		}

		/// <summary>
		/// Render messages in plain text to a file.
		/// </summary>
		/// <param name="fileName">The file name to write to.</param>
		/// <param name="logFileMode">Append or truncate log file</param>
		public TinyLoggerOptions AddFile(string fileName, LogFileMode logFileMode)
		{
			ArgumentNullException.ThrowIfNull(options);

			options.Renderers.Add(new FileRenderer(fileName, logFileMode));

			return options;
		}

		/// <summary>
		/// Render messages in plain text to a file with a rolling filename.
		/// </summary>
		/// <param name="getFileName">Retrieve a filename to write to, if the value changes a new file with that file name will be created.</param>
		public TinyLoggerOptions AddRollingFile(Func<string> getFileName)
		{
			return options.AddRollingFile(getFileName, LogFileMode.Append);
		}

		/// <summary>
		/// Render messages in plain text to a file with a rolling filename.
		/// </summary>
		/// <param name="getFileName">Retrieve a filename to write to, if the value changes a new file with that file name will be created.</param>
		/// <param name="logFileMode">Append or truncate log file</param>
		public TinyLoggerOptions AddRollingFile(Func<string> getFileName, LogFileMode logFileMode)
		{
			ArgumentNullException.ThrowIfNull(options);

			options.Renderers.Add(new FileRenderer(getFileName, logFileMode));

			return options;
		}

		/// <summary>
		/// Render messages in plain text to a stream.
		/// </summary>
		/// <param name="stream">The Stream to write to.</param>
		public TinyLoggerOptions AddStream(Stream stream)
		{
			ArgumentNullException.ThrowIfNull(options);

			options.Renderers.Add(new StreamRenderer(stream));

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

		System.Console.WriteLine($"forceColor: {forceColor}, noColor: {noColor}, isOutputRedirected: {System.Console.IsOutputRedirected}");

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
