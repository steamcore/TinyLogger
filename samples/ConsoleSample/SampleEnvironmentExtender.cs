using TinyLogger;

namespace ConsoleSample;

public class SampleEnvironmentExtender : ILogExtender
{
	public void Extend(Dictionary<string, MessageToken?> data)
	{
		if (data is null)
		{
			return;
		}

		data.TryAdd("hostname", new LiteralToken(Environment.MachineName));
		data.TryAdd("os", new ObjectToken<OperatingSystem>(Environment.OSVersion));
		data.TryAdd("runtime_version", new ObjectToken<Version>(Environment.Version));
	}
}
