using System.Diagnostics;

namespace TinyLogger.Extenders;

public class ProcessExtender : ILogExtender
{
	private static readonly DateTime startTime = Process.GetCurrentProcess().StartTime.ToUniversalTime();
	private static readonly FuncToken processTime = new(() => new LiteralToken((DateTime.UtcNow - startTime).ToString()));

	public void Extend(Dictionary<string, MessageToken?> data)
	{
		if (data?.ContainsKey("process_time") == false)
		{
			data.Add("process_time", processTime);
		}
	}
}
