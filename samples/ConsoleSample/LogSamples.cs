using Figgle;
using Microsoft.Extensions.Logging;

namespace ConsoleSample;

public class LogSamples(ILogger logger)
{
	public void LogAll()
	{
		LogIntro();
		LogSimpleExamples();
		LogComplexExamples();
		LogAllLevels();
		LogException();
	}

	public void LogIntro()
	{
		logger.LogInformation(new EventId(0, "Intro"), "{figgle}", Environment.NewLine + FiggleFonts.Slant.Render("TinyLogger"));
	}

	public void LogSimpleExamples()
	{
		var eventId = new EventId(0, "SimpleExamples");

		logger.LogInformation(eventId, "Example {true} / {false}", true, false);
		logger.LogInformation(eventId, "Example {int} / {long} / {float} / {double} / {decimal}", 1234, 1234L, 123.4f, 123.4, 123.4M);
		logger.LogInformation(eventId, "Example {string}", "foobar");
		logger.LogInformation(eventId, "Example {datetime} / {timespan}", DateTime.UtcNow, TimeSpan.FromMilliseconds(123456789));
		logger.LogInformation(eventId, "Example {guid}", Guid.NewGuid());
		logger.LogInformation(eventId, "Example {url}", new Uri("https://www.example.com/"));
		logger.LogInformation(eventId, "Example {version}", new Version("1.4.0.0"));
	}

	public void LogComplexExamples()
	{
		var eventId = new EventId(0, "ComplexExamples");

		logger.LogInformation(eventId,
			"Example tuple {tuple}",
			("foobar", 123, new Uri("https://www.example.com"))
		);

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

		logger.LogInformation(
			eventId,
			"Example lookup {lookup}",
			new (string key, object value)[]
			{
				("someValue", 12),
				("someValue", 34),
				("someValue", 56),
				("text", "lorem"),
				("text", "ipsum"),
				("timestamp", DateTime.UtcNow),
				("timestamp", DateTime.UtcNow.AddDays(1))
			}
			.ToLookup(x => x.key, x => x.value)
		);
	}

	public void LogAllLevels()
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

	public void LogException()
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
}
