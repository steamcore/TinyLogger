using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GenericHostSample
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			using var host = CreateHostBuilder(args).Build();

			await host.RunAsync();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging(builder =>
				{
					builder.AddTinyConsoleLogger();
				});
	}
}
