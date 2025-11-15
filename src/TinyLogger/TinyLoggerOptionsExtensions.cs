using TinyLogger.Console;
using TinyLogger.Console.TrueColor;
using TinyLogger.IO;

namespace TinyLogger;

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

			// Honor no-color.org
			if (Environment.GetEnvironmentVariable("NO_COLOR") != null)
			{
				options.Renderers.Add(new PlainTextConsoleRenderer());
			}
			else if (AnsiSupport.TryEnable() || Environment.GetEnvironmentVariable("ANSI_COLOR") != null)
			{
				options.Renderers.Add(new AnsiConsoleRenderer(theme));
			}
			else
			{
				options.Renderers.Add(new WindowsConsoleRenderer(theme));
			}

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

			// Honor no-color.org
			if (Environment.GetEnvironmentVariable("NO_COLOR") != null || !AnsiSupport.TryEnable())
			{
				options.Renderers.Add(new PlainTextConsoleRenderer());
			}
			else
			{
				options.Renderers.Add(new TrueColorConsoleRenderer(theme));
			}

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

			options.Renderers.Add(new RollingFileRenderer(getFileName, logFileMode));

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
}
