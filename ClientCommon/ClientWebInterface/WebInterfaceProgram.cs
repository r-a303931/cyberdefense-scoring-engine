using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ClientCommon.ClientService;
using ClientCommon.Data.Config;
using ClientCommon.Data.InformationContext;

namespace ClientCommon.WebInterface
{
    public class WebInterfaceProgram
    {
        public static IHostBuilder CreateHostBuilder(string[] args, IScriptProvider scriptProvider, IConfigurationManager configurationManager, IClientInformationContext clientInformationContext) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton(scriptProvider);
                    services.AddSingleton(configurationManager);
                    services.AddSingleton(clientInformationContext);
                    services.AddHostedService<VerificationService>();
                });
    }
}
