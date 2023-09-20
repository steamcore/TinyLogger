using System.Diagnostics;

namespace TinyLogger.Extenders;

public class ProcessExtender : ILogExtender
{
	private static readonly DateTime startTime = Process.GetCurrentProcess().StartTime.ToUniversalTime();

	public void Extend(Dictionary<string, object?> data)
	{
		if (data?.ContainsKey("process_time") == false)
		{
			data.Add("process_time", MessageToken.FromLiteral(DateTime.UtcNow - startTime));
		}
	}
}
