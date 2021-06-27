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

            EnsureUserIsRoot();

            CommandLine.Execute(
                args,
                Constants.Constants.InstallPath,
                Assembly.GetExecutingAssembly(),
                configurationManager,
                informationContext,
                new Hashtable
                {

                },
                async () =>
                {
                    await Common.ProcessManagement.RunProcessAsync(Process.Start(
                        "systemctl",
                        new string[] { "enable", "--now", "cyberdefense-scoring-engine" }
                    ));
                }
            ).Wait();
        }

        [DllImport("libc")]
        private static extern uint getuid();

        private static void EnsureUserIsRoot()
        {
            if (getuid() != 0)
            {
                Console.Error.WriteLine("Rerun the command as root");
                throw new Exception();
            }
        }
    }
}
