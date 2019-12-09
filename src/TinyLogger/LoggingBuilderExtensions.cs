using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TinyLogger;
using TinyLogger.Console;
using TinyLogger.Console.TrueColor;

namespace Microsoft.Extensions.Logging
{
	public static class LoggingBuilderExtensions
	{
		/// <summary>
		/// Add TinyLogger with a message template and color console renderer.
		/// </summary>
		public static ILoggingBuilder AddTinyConsoleLogger(this ILoggingBuilder logging, string? template = null, IConsoleTheme? theme = null)
		{
			return logging.AddTinyLogger(options =>
			{
				options.AddConsole(theme ?? new DefaultConsoleTheme());
				options.Template = template ?? MessageTemplates.Default;
			});
		}

		/// <summary>
		/// Add TinyLogger with a message template and plain text console renderer.
		/// </summary>
		public static ILoggingBuilder AddTinyPlainTextConsoleLogger(this ILoggingBuilder logging, string? template = null)
		{
			return logging.AddTinyLogger(options =>
			{
				options.AddPlainTextConsole();
				options.Template = template ?? MessageTemplates.Default;
			});
		}

		/// <summary>
		/// Add TinyLogger with a message template and RGB color console renderer.
		/// </summary>
		public static ILoggingBuilder AddTinyTrueColorConsoleLogger(this ILoggingBuilder logging, string? template = null, ITrueColorConsoleTheme? theme = null)
		{
			return logging.AddTinyLogger(options =>
			{
				options.AddTrueColorConsole(theme ?? new DefaultTrueColorConsoleTheme());
				options.Template = template ?? MessageTemplates.Default;
			});
		}

		/// <summary>
		/// Add TinyLogger and configure options. Make sure to add at least one renderer.
		/// </summary>
		/// <param name="configureOptions">A callback to configure options</param>
		public static ILoggingBuilder AddTinyLogger(this ILoggingBuilder logging, Action<TinyLoggerOptions> configureOptions)
		{
			logging.Services.AddOptions<TinyLoggerOptions>().Configure(configureOptions);
			logging.Services.AddSingleton<IValidateOptions<TinyLoggerOptions>, TinyLoggerOptionsValidator>();
			logging.Services.AddSingleton<ILoggerProvider, TinyLoggerProvider>();
			return logging;
		}
	}
}
