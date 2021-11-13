#pragma warning disable CA1812 // Workaround for analyzer bug https://github.com/dotnet/roslyn-analyzers/issues/5628
using ConsoleSample;
using Microsoft.Extensions.Logging;
using TinyLogger;

using var loggerFactory = LoggerFactory.Create(builder =>
{
	// Log everything for this example
	builder.SetMinimumLevel(LogLevel.Trace);

	// AddTinyConsoleLogger is the simplest way to add the console logger
	// builder.AddTinyConsoleLogger();

	// For customization, use AddTinyLogger
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
		//options.AddFile("sample.log");

		// Render to files with rolling names by uncommenting this, when the timestamp changes the file changes
		//options.AddRollingFile(() => $"sample-{DateTime.Now:yyyyMMdd-HHmm}.log");
	});
});

var samples = new LogSamples(loggerFactory.CreateLogger<LogSamples>());

samples.LogAll();
