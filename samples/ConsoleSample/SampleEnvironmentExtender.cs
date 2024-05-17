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
		data.TryAdd("os", new ObjectToken(Environment.OSVersion));
		data.TryAdd("runtime_version", new ObjectToken(Environment.Version));
	}
}
