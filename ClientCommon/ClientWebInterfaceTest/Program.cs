using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ClientCommon.ClientService;
using ClientCommon.Data.Config;
using ClientCommon.Data.InformationContext;
using ClientCommon.WebInterface;
using ClientWebInterfaceTest.Stubs;

namespace ClientWebInterfaceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var provider = new ScriptProviderStub();
            var conf = new TestConfigurationManager();
            var context = new TestClientInformationContext();

            context.SetSystemIdentifierAsync((int)conf.LoadConfiguration().Result.SystemIdentifier).Wait();

            WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddSingleton<IScriptProvider>(provider);
                    services.AddSingleton<IConfigurationManager>(conf);
                    services.AddSingleton<IClientInformationContext>(context);
                    services.AddHostedService<VerificationService>();
                })
                .UseWebRoot("wwwroot")
                .UseStartup<Startup>()
                .Build()
                .Run();
        }
    }
}

//            app.UseStaticFiles(new StaticFileOptions
//            {
//                FileProvider = new EmbeddedFileProvider(
//                    assembly: Assembly.GetAssembly(typeof(Startup)),
//                    baseNamespace: "ClientCommon.Startup.wwwroot")
//            });
