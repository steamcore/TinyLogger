using TinyLogger.Console;
using TinyLogger.Formatters;

namespace TinyLogger;

public class TinyLoggerOptionsValidatorTests
{
	[Test]
	public void Validate_should_allow_valid_options()
	{
		var options = new TinyLoggerOptions
		{
			MaxQueueDepth = 1000,
			Renderers = [new ConsoleRenderer(PlainTextLogFormatter.Instance)],
			Template = MessageTemplates.DefaultTimestamped
		};

		var validator = new TinyLoggerOptionsValidator();
		var result = validator.Validate(null, options);

		result.Succeeded.ShouldBeTrue();
	}

	[Test]
	public void Validate_should_deny_invalid_options()
	{
		var options = new TinyLoggerOptions
		{
			MaxQueueDepth = 0,
			Renderers = [],
			Template = ""
		};

		var validator = new TinyLoggerOptionsValidator();
		var result = validator.Validate(null, options);

		result.Failed.ShouldBeTrue();
		result.Failures.ShouldNotBeNull();
		result.Failures.Count().ShouldBe(3);
		result.Failures.ShouldContain(x => x.Contains(nameof(TinyLoggerOptions.MaxQueueDepth)));
		result.Failures.ShouldContain(x => x.Contains(nameof(TinyLoggerOptions.Renderers)));
		result.Failures.ShouldContain(x => x.Contains(nameof(TinyLoggerOptions.Template)));
	}
}
