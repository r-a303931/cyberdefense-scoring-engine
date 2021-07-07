using Microsoft.Extensions.Hosting;

using ClientCommon.Data.Config;
using ClientCommon.Data.InformationContext;
using ClientCommon.WebInterface;

using Clients.Linux.Constants;

namespace Clients.Linux.Main
{
    public class Program
    {
        static void Main(string[] args)
        {
            IConfigurationManager configurationManager = new LinuxFileConfigurationManager();
            IClientInformationContext informationContext = new TcpClientInformationClientContext(configurationManager);

            Util.EnsureUserIsRoot();

            WebInterfaceProgram
                .CreateHostBuilder(
                    args,
                    new LinuxScriptProvider(),
                    configurationManager,
                    informationContext
                )
                .Build()
                .Run();
        }
    }
}
