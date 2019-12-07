using Figgle;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TinyLogger;

namespace ConsoleSample
{
	public class Program
	{
		public static void Main(string[] args)
		{
			using var loggerFactory = LoggerFactory.Create(builder =>
			{
				// Log everything for this example
				builder.SetMinimumLevel(LogLevel.Trace);

				builder.AddTinyLogger(options =>
				{
					// Optionally extend log fields with new or modified data
					options.Extenders.Add(new SampleExceptionExtender());

					// Select a custom message template
					options.Template = MessageTemplates.DefaultTimestamped;

					// Render to console
					options.AddConsole();

					// Render to file with rolling name, when the timestamp changes the file changes
					options.AddFile(() => $"example-{DateTime.Now.ToString("yyyyMMdd-HHmm")}.log");
				});
			});

			var logger = loggerFactory.CreateLogger(typeof(Program));

			logger.LogInformation(new EventId(0, "Intro"), Environment.NewLine + FiggleFonts.Slant.Render("TinyLogger"));

			LogSimpleExamples(logger);
			LogComplexExamples(logger);
			LogAllLevels(logger);
			LogException(logger);
		}

		private static void LogSimpleExamples(ILogger logger)
		{
			var eventId = new EventId(0, "SimpleExamples");

			logger.LogInformation(eventId, "Example {number}", 1234);
			logger.LogInformation(eventId, "Example {string}", "foobar");
			logger.LogInformation(eventId, "Example {datetime}", DateTime.UtcNow);
			logger.LogInformation(eventId, "Example {datetime:o}", DateTime.UtcNow);
			logger.LogInformation(eventId, "Example {guid}", Guid.NewGuid());
			logger.LogInformation(eventId, "Example {url}", new Uri("https://www.example.com/"));
		}

		private static void LogComplexExamples(ILogger logger)
		{
			var eventId = new EventId(0, "ComplexExamples");

			logger.LogInformation(
				eventId,
				"Example list {list}",
				new List<object>
				{
					1234,
					"example string",
					new Uri("https://www.example.com/")
				}
			);

			logger.LogInformation(
				eventId,
				"Example dictionary {dictionary}",
				new Dictionary<string, object>
				{
					{ "someValue", 1234 },
					{ "text", "lorem ipsum" },
					{ "timestamp", DateTime.UtcNow }
				}
			);
		}

		private static void LogAllLevels(ILogger logger)
		{
			var eventId = new EventId(0, "LogLevels");

			var logLevels = new[]
			{
				LogLevel.Trace,
				LogLevel.Debug,
				LogLevel.Information,
				LogLevel.Warning,
				LogLevel.Error,
				LogLevel.Critical
			};

			foreach (var logLevel in logLevels)
			{
				logger.Log(logLevel, eventId, "Example message with log level {logLevel}", logLevel);
			}
		}

		private static void LogException(ILogger logger)
		{
			var eventId = new EventId(0, "Exceptions");

			try
			{
				InternalMethodA();
			}
			catch (Exception ex)
			{
				logger.LogWarning(eventId, ex, "Caught example exception");
				logger.LogError(eventId, ex, "Caught example exception");
			}

			void InternalMethodA()
			{
				InternalMethodB();
			}

			void InternalMethodB()
			{
				throw new InvalidOperationException("Example error message");
			}
		}

		private class SampleExceptionExtender : ILogExtender
		{
			public void Extend(Dictionary<string, object> data)
			{
				if (data.ContainsKey("exception") && data["exception"] is Exception exception)
				{
					exception.Demystify();
				}
			}
		}
	}
}
