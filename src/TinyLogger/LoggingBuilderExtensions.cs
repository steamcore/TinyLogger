using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TinyLogger;
using TinyLogger.Formatters;
using TinyLogger.IO;
using TinyLogger.Themes.AnsiColorTheme;
using TinyLogger.Themes.TrueColorTheme;

namespace Microsoft.Extensions.Logging;

public static class LoggingBuilderExtensions
{
	extension(ILoggingBuilder logging)
	{
		/// <summary>
		/// Add TinyLogger with a message template and color console renderer.
		/// </summary>
		public ILoggingBuilder AddTinyConsoleLogger(string? template = null, IAnsiColorTheme? theme = null)
		{
			return logging.AddTinyLogger(options =>
			{
				options.AddConsole(theme ?? new DefaultAnsiColorTheme());
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
		public ILoggingBuilder AddTinyTrueColorConsoleLogger(string? template = null, ITrueColorTheme? theme = null)
		{
			return logging.AddTinyLogger(options =>
			{
				options.AddTrueColorConsole(theme ?? new DefaultTrueColorTheme());
				options.Template = template ?? MessageTemplates.Default;
			});
		}

		/// <summary>
		/// Adds TinyLogger configured with a file renderer.
		/// </summary>
		public ILoggingBuilder AddTinyFileLogger(string fileName, LogFileMode logFileMode = LogFileMode.Append, string? template = null)
		{
			return AddTinyFileLogger(logging, fileName, PlainTextLogFormatter.Instance, logFileMode, template);
		}

		/// <summary>
		/// Adds TinyLogger configured with a file renderer.
		/// </summary>
		public ILoggingBuilder AddTinyFileLogger(string fileName, ILogFormatter formatter, LogFileMode logFileMode = LogFileMode.Append, string? template = null)
		{
			return logging.AddTinyLogger(options =>
			{
				options.AddFile(fileName, formatter, logFileMode);
				options.Template = template ?? MessageTemplates.Default;
			});
		}

		/// <summary>
		/// Adds TinyLogger configured with a rolling file renderer.
		/// </summary>
		public ILoggingBuilder AddTinyRollingFileLogger(Func<string> getFileName, LogFileMode logFileMode = LogFileMode.Append, string? template = null)
		{
			return AddTinyRollingFileLogger(logging, getFileName, PlainTextLogFormatter.Instance, logFileMode, template);
		}

		/// <summary>
		/// Adds TinyLogger configured with a rolling file renderer.
		/// </summary>
		public ILoggingBuilder AddTinyRollingFileLogger(Func<string> getFileName, ILogFormatter formatter, LogFileMode logFileMode = LogFileMode.Append, string? template = null)
		{
			return logging.AddTinyLogger(options =>
			{
				options.AddRollingFile(getFileName, formatter, logFileMode);
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

			var foundValidator = false;
			var foundProvider = false;

			// Detect if the options validator and logger provider have already been registered to avoid duplicate registrations,
			// we do it this way because we can not use TryAddSingleton as that would fail to register the services if there are other services with the same service type.
			foreach (var service in logging.Services)
			{
				if (service.ImplementationType == typeof(TinyLoggerOptionsValidator))
				{
					foundValidator = true;
				}
				else if (service.ImplementationType == typeof(TinyLoggerProvider))
				{
					foundProvider = true;
				}

				if (foundValidator && foundProvider)
				{
					break;
				}
			}

			if (!foundValidator)
			{
				logging.Services.AddSingleton<IValidateOptions<TinyLoggerOptions>, TinyLoggerOptionsValidator>();
			}

			if (!foundProvider)
			{
				logging.Services.AddSingleton<ILoggerProvider, TinyLoggerProvider>();
			}

			return logging;
		}
	}
}
