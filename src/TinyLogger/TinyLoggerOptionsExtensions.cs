using System;
using TinyLogger.Console;
using TinyLogger.Files;

namespace TinyLogger
{
	public static class TinyLoggerOptionsExtensions
	{
		/// <summary>
		/// Render messages to the console using the default color theme.
		/// </summary>
		public static TinyLoggerOptions AddConsole(this TinyLoggerOptions options)
		{
			options.Renderers.Add(new ConsoleRenderer());
			return options;
		}

		/// <summary>
		/// Render messages to the console using a specific color theme.
		/// </summary>
		public static TinyLoggerOptions AddConsole(this TinyLoggerOptions options, IConsoleTheme theme)
		{
			options.Renderers.Add(new ConsoleRenderer(theme));
			return options;
		}

		/// <summary>
		/// Render messages in plain text to a file.
		/// </summary>
		/// <param name="fileName">The file name to write to.</param>
		public static TinyLoggerOptions AddFile(this TinyLoggerOptions options, string fileName)
		{
			return options.AddFile(() => fileName);
		}

		/// <summary>
		/// Render messages in plain text to a file.
		/// </summary>
		/// <param name="getFileName">Retrieve a filename to write to, if the value changes a new file with that file name will be created.</param>
		public static TinyLoggerOptions AddFile(this TinyLoggerOptions options, Func<string> getFileName)
		{
			options.Renderers.Add(new FileRenderer(getFileName));
			return options;
		}
	}
}
