using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TinyLogger;
using TinyLogger.Console;
using TinyLogger.Console.TrueColor;

namespace Microsoft.Extensions.Logging;

public static class LoggingBuilderExtensions
{
	extension(ILoggingBuilder logging)
	{
		/// <summary>
		/// Add TinyLogger with a message template and color console renderer.
		/// </summary>
		public ILoggingBuilder AddTinyConsoleLogger(string? template = null, IConsoleTheme? theme = null)
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
		public ILoggingBuilder AddTinyPlainTextConsoleLogger(string? template = null)
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
		public ILoggingBuilder AddTinyTrueColorConsoleLogger(string? template = null, ITrueColorConsoleTheme? theme = null)
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
		public ILoggingBuilder AddTinyLogger(Action<TinyLoggerOptions> configureOptions)
		{
			ArgumentNullException.ThrowIfNull(logging);

			logging.Services.AddOptions<TinyLoggerOptions>().Configure(configureOptions);
			logging.Services.AddSingleton<IValidateOptions<TinyLoggerOptions>, TinyLoggerOptionsValidator>();
			logging.Services.AddSingleton<ILoggerProvider, TinyLoggerProvider>();
			return logging;
		}
	}
}
