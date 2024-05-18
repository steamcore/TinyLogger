using Microsoft.Extensions.Logging;

namespace TinyLogger.Extenders;

public class LogLevelExtender : ILogExtender
{
	private static readonly Dictionary<LogLevel, ObjectToken<LogLevel>> logLevelTokens = new()
	{
		[LogLevel.Trace] = new ObjectTokenWithTransform<LogLevel>(LogLevel.Trace, GetShortLogLevel),
		[LogLevel.Debug] = new ObjectTokenWithTransform<LogLevel>(LogLevel.Debug, GetShortLogLevel),
		[LogLevel.Information] = new ObjectTokenWithTransform<LogLevel>(LogLevel.Information, GetShortLogLevel),
		[LogLevel.Warning] = new ObjectTokenWithTransform<LogLevel>(LogLevel.Warning, GetShortLogLevel),
		[LogLevel.Error] = new ObjectTokenWithTransform<LogLevel>(LogLevel.Error, GetShortLogLevel),
		[LogLevel.Critical] = new ObjectTokenWithTransform<LogLevel>(LogLevel.Critical, GetShortLogLevel),
		[LogLevel.None] = new ObjectTokenWithTransform<LogLevel>(LogLevel.None, GetShortLogLevel)
	};

	public void Extend(Dictionary<string, MessageToken?> data)
	{
		if (data?.TryGetValue("logLevel", out var token) == true && token?.TryGetValue<LogLevel>(out var logLevel) == true)
		{
			data["logLevel_short"] = logLevelTokens[logLevel];
		}
	}

	private static string GetShortLogLevel(LogLevel logLevel)
	{
		return logLevel switch
		{
			LogLevel.Trace => "trce",
			LogLevel.Debug => "dbug",
			LogLevel.Information => "info",
			LogLevel.Warning => "warn",
			LogLevel.Error => "fail",
			LogLevel.Critical => "crit",
			_ => ""
		};
	}
}
