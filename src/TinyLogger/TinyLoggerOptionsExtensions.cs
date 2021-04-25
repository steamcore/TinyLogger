using System;
using System.IO;
using TinyLogger.Console;
using TinyLogger.Console.TrueColor;
using TinyLogger.IO;

namespace TinyLogger
{
	public static class TinyLoggerOptionsExtensions
	{
		/// <summary>
		/// Render messages to the console using the default color theme.
		/// </summary>
		public static TinyLoggerOptions AddConsole(this TinyLoggerOptions options)
		{
			return AddConsole(options, new DefaultConsoleTheme());
		}

		/// <summary>
		/// Render messages to the console using a specific color theme.
		/// </summary>
		public static TinyLoggerOptions AddConsole(this TinyLoggerOptions options, IConsoleTheme theme)
		{
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
		public static TinyLoggerOptions AddPlainTextConsole(this TinyLoggerOptions options)
		{
			options.Renderers.Add(new PlainTextConsoleRenderer());
			return options;
		}

		/// <summary>
		/// Render messages to the console using the default RGB color theme.
		/// </summary>
		public static TinyLoggerOptions AddTrueColorConsole(this TinyLoggerOptions options)
		{
			return AddTrueColorConsole(options, new DefaultTrueColorConsoleTheme());
		}

		/// <summary>
		/// Render messages to the console using a specific RGB color theme.
		/// </summary>
		public static TinyLoggerOptions AddTrueColorConsole(this TinyLoggerOptions options, ITrueColorConsoleTheme theme)
		{
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
		public static TinyLoggerOptions AddFile(this TinyLoggerOptions options, string fileName)
		{
			return options.AddFile(fileName, FileMode.Append);
		}

		/// <summary>
		/// Render messages in plain text to a file.
		/// </summary>
		/// <param name="fileName">The file name to write to.</param>
		public static TinyLoggerOptions AddFile(this TinyLoggerOptions options, string fileName, FileMode fileMode)
		{
			options.Renderers.Add(new FileRenderer(fileName, fileMode));
			return options;
		}

		/// <summary>
		/// Render messages in plain text to a file with a rolling filename.
		/// </summary>
		/// <param name="getFileName">Retrieve a filename to write to, if the value changes a new file with that file name will be created.</param>
		[Obsolete("Use AddRollingFile")]
		public static TinyLoggerOptions AddFile(this TinyLoggerOptions options, Func<string> getFileName)
		{
			return options.AddRollingFile(getFileName);
		}

		/// <summary>
		/// Render messages in plain text to a file with a rolling filename.
		/// </summary>
		/// <param name="getFileName">Retrieve a filename to write to, if the value changes a new file with that file name will be created.</param>
		public static TinyLoggerOptions AddRollingFile(this TinyLoggerOptions options, Func<string> getFileName)
		{
			return options.AddRollingFile(getFileName, FileMode.Append);
		}

		/// <summary>
		/// Render messages in plain text to a file with a rolling filename.
		/// </summary>
		/// <param name="getFileName">Retrieve a filename to write to, if the value changes a new file with that file name will be created.</param>
		public static TinyLoggerOptions AddRollingFile(this TinyLoggerOptions options, Func<string> getFileName, FileMode fileMode)
		{
			options.Renderers.Add(new RollingFileRenderer(getFileName, fileMode));
			return options;
		}

		/// <summary>
		/// Render messages in plain text to a stream.
		/// </summary>
		/// <param name="stream">The Stream to write to.</param>
		public static TinyLoggerOptions AddStream(this TinyLoggerOptions options, Stream stream)
		{
			options.Renderers.Add(new StreamRenderer(stream));
			return options;
		}
	}
}
