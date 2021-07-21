using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using ClientCommon.Data.Config;
using ClientCommon.Data.InformationContext;
using ClientCommon.Installer.Utilities;

using Clients.Linux.Constants;

namespace Clients.Linux.Installer
{
    /// <summary>
    /// This program installs and configures the cyber defense scoring engine
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            IConfigurationManager configurationManager = new LinuxFileConfigurationManager();
            IClientInformationContext informationContext = new TcpClientInformationClientContext(configurationManager);

            Util.EnsureUserIsRoot();

            CommandLine
                .Execute(
                    args,
                    Constants.Constants.InstallPath,
                    Assembly.GetExecutingAssembly(),
                    configurationManager,
                    informationContext,
                    new Hashtable
                    {
                        { "Clients.Linux.Installer.Resources.cdse.service", "/etc/systemd/system/cdse.service" },
                    },
                    async () =>
                    {
                        await Common.ProcessManagement.RunProcessAsync(Process.Start(
                            "systemctl", new string[] { "daemon-reload" }
                        ));
                        await Common.ProcessManagement.RunProcessAsync(Process.Start(
                            "systemctl",
                            new string[] { "enable", "--now", "cyberdefense-scoring-engine" }
                        ));
                    }
                )
                .Wait();
        }
    }
}
