using Microsoft.Extensions.Logging;

namespace TinyLogger.Extenders;

public class LogLevelExtender : ILogExtender
{
	public void Extend(Dictionary<string, MessageToken?> data)
	{
		if (data?.TryGetValue("logLevel", out var token) == true && token?.TryGetValue<LogLevel>(out var logLevel) == true)
		{
			data["logLevel_short"] = new ObjectTokenWithTransform<LogLevel>(logLevel, GetShortLogLevel);
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
