using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GenericHostSample
{
	class Program
	{
		static async Task Main(string[] args)
		{
			using var host = CreateHostBuilder(args).Build();

			await host.StartAsync();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging(builder =>
				{
					builder.AddTinyConsoleLogger();
				});
	}
}
