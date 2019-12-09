using System;
using Microsoft.Extensions.Logging;
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
					//options.AddPlainTextConsole();
					//options.AddTrueColorConsole();

					// Render to file with rolling name by uncommenting this, when the timestamp changes the file changes
					//options.AddFile(() => $"example-{DateTime.Now.ToString("yyyyMMdd-HHmm")}.log");
				});
			});

			var samples = new LogSamples(loggerFactory.CreateLogger<LogSamples>());

			samples.LogAll();
		}
	}
}
