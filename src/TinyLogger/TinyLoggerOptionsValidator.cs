using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace TinyLogger
{
	public class TinyLoggerOptionsValidator : IValidateOptions<TinyLoggerOptions>
	{
		public ValidateOptionsResult Validate(string name, TinyLoggerOptions options)
		{
			var failures = new List<string>();

			if (options.MaxQueueDepth < 1)
				failures.Add("TinyLoggerOptions max queue depth must be at least 1");

			if (options.Renderers.Count == 0)
				failures.Add("TinyLoggerOptions is missing renderers");

			if (string.IsNullOrWhiteSpace(options.Template))
				failures.Add("TinyLoggerOptions template is empty");

			return failures.Count > 0
				? ValidateOptionsResult.Fail(failures)
				: ValidateOptionsResult.Success;
		}
	}
}
