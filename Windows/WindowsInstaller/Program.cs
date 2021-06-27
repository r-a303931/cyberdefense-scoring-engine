using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;

using ClientCommon.Data.Config;
using ClientCommon.Data.InformationContext;
using ClientCommon.Installer.Utilities;
using Clients.Windows.Constants;

namespace Clients.Windows.Installer
{
    /// <summary>
    /// This program installs and configures the cyber defense scoring engine
    /// </summary>
    public class Program
    {
        static void Main(string[] args)
        {
            IConfigurationManager configurationManager = new WindowsFileConfigurationManager();
            IClientInformationContext informationContext = new TcpClientInformationClientContext(configurationManager);

            CommandLine.Execute(
                args,
                Constants.Constants.InstallPath,
                Assembly.GetExecutingAssembly(),
                configurationManager,
                informationContext,
                new Hashtable
                {
                    { "Clients.Windows.Installer.Resources.README.url", @"C:\Users\Public\Desktop\README.url" },
                    { "Clients.Windows.Installer.Resources.ScoringReport.url", @"C:\Users\Public\Desktop\ScoringReport.url" }
                },
                async () =>
                {
                    await Common.ProcessManagement.RunProcessAsync(Process.Start(
                        "sc.exe",
                        new string[] { "create", "CDSE", "start=", "auto", "error=", "normal", "binpath=", @"C:\CDSE\WindowsClient.exe" }
                    ));
                    // sc.exe create CDSE start= auto error= normal binpath= C:\CDSE\WindowsClient.exe
                }
            ).Wait();
        }
    }
}
