using Microsoft.Extensions.Hosting;

using ClientCommon.Data.Config;
using ClientCommon.Data.InformationContext;
using ClientCommon.WebInterface;

using Clients.Windows.Constants;

namespace Clients.Windows.Main
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfigurationManager configurationManager = new WindowsFileConfigurationManager();
            IClientInformationContext informationContext = new TcpClientInformationClientContext(configurationManager);

            informationContext.ConnectAsync().Wait();

            WebInterfaceProgram
                .CreateHostBuilder(
                    args,
                    new WindowsScriptProvider(),
                    configurationManager,
                    informationContext
                )
                .Build()
                .Run();
        }
    }
}
