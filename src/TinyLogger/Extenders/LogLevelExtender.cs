using Microsoft.Extensions.Logging;

namespace TinyLogger.Extenders;

public class LogLevelExtender : ILogExtender
{
	public void Extend(Dictionary<string, object?> data)
	{
		if (data.ContainsKey("logLevel") && data["logLevel"] is LogLevel logLevel)
		{
			data["logLevel_short"] = MessageToken.FromObject(logLevel, valueTransform: x => GetShortLogLevel((LogLevel)x));
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
