using TinyLogger;

namespace ConsoleSample;

public class SampleEnvironmentExtender : ILogExtender
{
	private static readonly LiteralToken machineName = new(Environment.MachineName);
	private static readonly ObjectToken<OperatingSystem> os = new(Environment.OSVersion);
	private static readonly ObjectToken<Version> runtimeVersion = new(Environment.Version);

	public void Extend(Dictionary<string, MessageToken?> data)
	{
		if (data is null)
		{
			return;
		}

		data.TryAdd("hostname", machineName);
		data.TryAdd("os", os);
		data.TryAdd("runtime_version", runtimeVersion);
	}
}
