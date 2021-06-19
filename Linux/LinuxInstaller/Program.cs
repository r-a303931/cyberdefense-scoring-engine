using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using System.CommandLine;
using System.CommandLine.Invocation;

using ClientCommon.Data;
using ClientCommon.Data.Config;
using ClientCommon.Installer.Utilities;
using Clients.Linux.Constants;
using System.Runtime.InteropServices;

namespace Clients.Linux.Installer
{
    /// <summary>
    /// This program installs and configures the cyber defense scoring engine
    /// </summary>
    public class Program
    {
        static int Main(string[] args)
        {
            IConfigurationManager configurationManager = new LinuxFileConfigurationManager();
            IClientInformationContext informationContext = new ClientInformationContext(configurationManager);

            var installCommand = new Command("install")
            {
                new Option<int?>(
                    new string[] { "--system-id", "-s" },
                    getDefaultValue: () => 0,
                    description: "Which system this should judge based off of"),
                new Option<int?>(
                    new string[] { "--team-id", "-t" },
                    getDefaultValue: () => 0,
                    description: "Preset team ID to prevent competition members from doing so themselves"),
                new Option<string>(
                    new string[] { "--controller-host", "-h" },
                    description: "")
            };

            installCommand.Handler = CommandHandler.Create(async (int? inputSystemId, int? inputTeamId, string inputControllerHost, CancellationToken token) =>
            {
                EnsureUserIsRoot();

                await Configuration.Configure(configurationManager, informationContext, inputSystemId, inputTeamId, inputControllerHost, token);
                await Configuration.InstallFiles(Assembly.GetExecutingAssembly(), Constants.Constants.InstallPath, cancellationToken: token);
            });

            var testConnectionCommand = new Command("test")
            {
                new Option<string>(new string[] { "--controller-host", "-h" })
            };

            testConnectionCommand.Handler = CommandHandler.Create(async (string host, CancellationToken cancellationToken) =>
            {
                var config = await configurationManager.LoadConfiguration(cancellationToken);

                var usedHost = config.EngineControllerHost is null
                    ? host
                    : config.EngineControllerHost;

                if (await informationContext.TestConnectionAsync(usedHost, verbose: true, cancellationToken: cancellationToken))
                {
                    Console.WriteLine("Connection successful");
                }
            });

            var rootCommand = new RootCommand
            {
                installCommand,
                testConnectionCommand
            };

            return rootCommand.Invoke(args);
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
