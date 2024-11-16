using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;
using TinyLogger.Console;
using TinyLogger.Console.TrueColor;
using TinyLogger.IO;

namespace TinyLogger.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "BenchmarkDotNet")]
public partial class LogComparisonBenchmarks : IDisposable
{
	private readonly ILoggerFactory dotnetConsoleFactory;
	private readonly ILogger dotnetConsole;

	private readonly ILoggerFactory ansiConsoleFactory;
	private readonly ILogger ansiConsole;

	private readonly ILoggerFactory plainTextConsoleFactory;
	private readonly ILogger plainTextConsole;

	private readonly ILoggerFactory trueColorConsoleFactory;
	private readonly ILogger trueColorConsole;

	private readonly ILoggerFactory streamFactory;
	private readonly ILogger stream;
	private readonly MemoryStream memoryStream = new();

	public LogComparisonBenchmarks()
	{
		dotnetConsoleFactory = LoggerFactory.Create(configure =>
		{
			configure.AddConsole();
		});

		dotnetConsole = dotnetConsoleFactory.CreateLogger<LogComparisonBenchmarks>();

		ansiConsoleFactory = LoggerFactory.Create(configure =>
		{
			configure.AddTinyLogger(options =>
			{
				options.Renderers.Add(new AnsiConsoleRenderer(new DefaultConsoleTheme()));
				options.UseSynchronousWrites = true;
			});
		});

		ansiConsole = ansiConsoleFactory.CreateLogger<LogComparisonBenchmarks>();

		plainTextConsoleFactory = LoggerFactory.Create(configure =>
		{
			configure.AddTinyLogger(options =>
			{
				options.Renderers.Add(new PlainTextConsoleRenderer());
				options.UseSynchronousWrites = true;
			});
		});

		plainTextConsole = plainTextConsoleFactory.CreateLogger<LogComparisonBenchmarks>();

		trueColorConsoleFactory = LoggerFactory.Create(configure =>
		{
			configure.AddTinyLogger(options =>
			{
				options.Renderers.Add(new TrueColorConsoleRenderer(new DefaultTrueColorConsoleTheme()));
				options.UseSynchronousWrites = true;
			});
		});

		trueColorConsole = trueColorConsoleFactory.CreateLogger<LogComparisonBenchmarks>();

		streamFactory = LoggerFactory.Create(configure =>
		{
			configure.AddTinyLogger(options =>
			{
				options.Renderers.Add(new StreamRenderer(memoryStream));
				options.UseSynchronousWrites = true;
			});
		});

		stream = streamFactory.CreateLogger<LogComparisonBenchmarks>();
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			dotnetConsoleFactory.Dispose();
			ansiConsoleFactory.Dispose();
			plainTextConsoleFactory.Dispose();
			trueColorConsoleFactory.Dispose();
			streamFactory.Dispose();
			memoryStream.Dispose();
		}
	}

	[Benchmark(Baseline = true)]
	public void DotnetConsole()
	{
		LogMessage(dotnetConsole, "foobar", 42);
	}

	[Benchmark]
	public void AnsiConsole()
	{
		LogMessage(ansiConsole, "foobar", 42);
	}

	[Benchmark]
	public void PlainTextConsole()
	{
		LogMessage(plainTextConsole, "foobar", 42);
	}

	[Benchmark]
	public void TrueColorConsole()
	{
		LogMessage(trueColorConsole, "foobar", 42);
	}

	[Benchmark]
	public void Stream()
	{
		LogMessage(stream, "foobar", 42);
	}

	[LoggerMessage(0, LogLevel.Information, "Some log message {key} is {value}")]
	private static partial void LogMessage(ILogger logger, string key, int value);
}
