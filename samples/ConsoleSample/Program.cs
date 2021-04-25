using Microsoft.Extensions.Logging;
using TinyLogger;

namespace ConsoleSample
{
	public class Program
	{
		public static void Main()
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

					// Render to a single file by uncommenting this
					//options.AddFile("consolesample.log");

					// Render to files with rolling names by uncommenting this, when the timestamp changes the file changes
					//options.AddRollingFile(() => $"example-{DateTime.Now:yyyyMMdd-HHmm}.log");
				});
			});

			var samples = new LogSamples(loggerFactory.CreateLogger<LogSamples>());

			samples.LogAll();
		}
	}
}
