using System.Diagnostics;
using TinyLogger;

namespace ConsoleSample;

public class SampleExceptionExtender : ILogExtender
{
	public void Extend(Dictionary<string, object> data)
	{
		if (data.ContainsKey("exception") && data["exception"] is Exception exception)
		{
			exception.Demystify();
		}
	}
}
