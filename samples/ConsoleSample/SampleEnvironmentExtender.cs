using TinyLogger;

namespace ConsoleSample;

public class SampleEnvironmentExtender : ILogExtender
{
	public void Extend(Dictionary<string, object?> data)
	{
		if (data is null)
		{
			return;
		}

		data.TryAdd("hostname", Environment.MachineName);
		data.TryAdd("os", Environment.OSVersion);
		data.TryAdd("runtime_version", Environment.Version);
	}
}
