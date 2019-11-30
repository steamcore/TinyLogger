using System;
using Microsoft.Extensions.DependencyInjection;
using TinyLogger;

namespace Microsoft.Extensions.Logging
{
	public static class LoggingBuilderExtensions
	{
		/// <summary>
		/// Add TinyLogger with default options and console renderer.
		/// </summary>
		public static ILoggingBuilder AddTinyConsoleLogger(this ILoggingBuilder logging)
		{
			return logging.AddTinyLogger(new TinyLoggerOptions().AddConsole());
		}

		/// <summary>
		/// Add TinyLogger and configure options. Make sure to add at least one renderer.
		/// </summary>
		/// <param name="configureOptions">A callback to configure options</param>
		public static ILoggingBuilder AddTinyLogger(this ILoggingBuilder logging, Action<TinyLoggerOptions> configureOptions)
		{
			var options = new TinyLoggerOptions();
			configureOptions(options);
			return logging.AddTinyLogger(options);
		}

		/// <summary>
		/// Add TinyLogger with specified options.
		/// </summary>
		/// <param name="options">The options to use</param>
		public static ILoggingBuilder AddTinyLogger(this ILoggingBuilder logging, TinyLoggerOptions options)
		{
			logging.Services.AddSingleton<ILoggerProvider>(_ => new TinyLoggerProvider(options));
			return logging;
		}
	}
}
