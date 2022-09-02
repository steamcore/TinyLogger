using System.Diagnostics;
using TinyLogger;

namespace ConsoleSample;

public class SampleExceptionExtender : ILogExtender
{
	public void Extend(Dictionary<string, object?> data)
	{
		if (data.TryGetValue("exception", out var value) && value is Exception exception)
		{
			exception.Demystify();
		}
	}
}
