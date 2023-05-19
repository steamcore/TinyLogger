using TinyLogger;

namespace ConsoleSample;

public class SampleEnvironmentExtender : ILogExtender
{
	public void Extend(Dictionary<string, object?> data)
	{
		data.TryAdd("hostname", Environment.MachineName);
		data.TryAdd("os", Environment.OSVersion);
		data.TryAdd("runtime_version", Environment.Version);
	}
}
