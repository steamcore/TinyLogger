#pragma warning disable CA1812 // Workaround for analyzer bug https://github.com/dotnet/roslyn-analyzers/issues/5628
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using var host = Host.CreateDefaultBuilder(args)
	.ConfigureLogging(builder =>
	{
		// Add TinyLogger, remember to disable the default Console logger in appsettings.json
		builder.AddTinyConsoleLogger();
	})
	.Build();

await host.RunAsync();
