using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

using ClientCommon.Data.Config;
using ClientCommon.Data.InformationContext;

namespace ClientCommon.Installer.Utilities
{
    public class CommandLine
    {
        public async static Task Execute(string[] args, string installPath, Assembly assembly, IConfigurationManager configurationManager, IClientInformationContext informationContext, Hashtable fileMappings, Func<Task> extraSetup)
        {
            int? inputSystemId = null;
            int? inputTeamId = null;
            string inputControllerHost = null;

            if (args.Length == 0)
            {
                Console.WriteLine("No command specified. Commands available: install, test");
            }

            string command = args[0];

            for (var i = 1; i < args.Length; i++)
            {
                var arg = args[i];

                switch (arg)
                {
                    case "-c":
                    case "--controller-host":
                        inputControllerHost = args[i + 1];
                        i++;

                        break;

                    case "-t":
                    case "--team-id":
                        inputTeamId = int.Parse(args[i + 1]);
                        i++;

                        break;

                    case "-s":
                    case "--system-id":
                        inputSystemId = int.Parse(args[i + 1]);
                        i++;

                        break;
                }
            }

            switch (command)
            {
                case "test":
                    await ExecuteTest(inputControllerHost, configurationManager, informationContext);

                    break;

                case "install":
                    await ExecuteInstall(inputSystemId, inputTeamId, inputControllerHost, installPath, assembly, configurationManager, informationContext, fileMappings, extraSetup);

                    break;

                default:
                    throw new Exception("Invalid command specified! Valid commands: test, install");
            }
        }

        public async static Task ExecuteInstall(int? inputSystemId, int? inputTeamId, string inputControllerHost, string installPath, Assembly assembly, IConfigurationManager configurationManager, IClientInformationContext informationContext, Hashtable fileMappings, Func<Task> extraSetup)
        {
            var token = CancellationToken.None;
            try
            {
                Console.WriteLine($"Install path: {installPath}");
                Directory.CreateDirectory(installPath);

                await Configuration.Configure(configurationManager, informationContext, inputSystemId, inputTeamId, inputControllerHost, token);
                await Configuration.InstallFiles(assembly, installPath, fileMappings, cancellationToken: token);

                await extraSetup();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Could not perform setup:");
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
            }
        }

        public async static Task ExecuteTest(string inputControllerHost, IConfigurationManager configurationManager, IClientInformationContext informationContext)
        {
            var cancellationToken = CancellationToken.None;
            var config = await configurationManager.LoadConfiguration(cancellationToken);

            var usedHost = config.EngineControllerHost is null
                ? inputControllerHost
                : config.EngineControllerHost;

            if (await informationContext.TestConnectionAsync(usedHost, verbose: true, cancellationToken: cancellationToken))
            {
                Console.WriteLine("Connection successful");
            }
            else
            {
                Console.WriteLine("Connection failed");
            }
        }
    }
}
