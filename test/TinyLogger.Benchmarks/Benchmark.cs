using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;
using TinyLogger.Console;
using TinyLogger.Console.TrueColor;
using TinyLogger.IO;

namespace TinyLogger.Benchmarks;

[MemoryDiagnoser]
public partial class Benchmark : IDisposable
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
	private readonly MemoryStream memoryStream = new MemoryStream();

	public Benchmark()
	{
		dotnetConsoleFactory = LoggerFactory.Create(configure =>
		{
			configure.AddConsole();
		});

		dotnetConsole = dotnetConsoleFactory.CreateLogger<Benchmark>();

		ansiConsoleFactory = LoggerFactory.Create(configure =>
		{
			configure.AddTinyLogger(options =>
			{
				options.Renderers.Add(new AnsiConsoleRenderer(new DefaultConsoleTheme()));
			});
		});

		ansiConsole = ansiConsoleFactory.CreateLogger<Benchmark>();

		plainTextConsoleFactory = LoggerFactory.Create(configure =>
		{
			configure.AddTinyLogger(options =>
			{
				options.Renderers.Add(new PlainTextConsoleRenderer());
			});
		});

		plainTextConsole = plainTextConsoleFactory.CreateLogger<Benchmark>();

		trueColorConsoleFactory = LoggerFactory.Create(configure =>
		{
			configure.AddTinyLogger(options =>
			{
				options.Renderers.Add(new TrueColorConsoleRenderer(new DefaultTrueColorConsoleTheme()));
			});
		});

		trueColorConsole = trueColorConsoleFactory.CreateLogger<Benchmark>();

		streamFactory = LoggerFactory.Create(configure =>
		{
			configure.AddTinyLogger(options =>
			{
				options.Renderers.Add(new StreamRenderer(memoryStream));
			});
		});

		stream = streamFactory.CreateLogger<Benchmark>();
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
