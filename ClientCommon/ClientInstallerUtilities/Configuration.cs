using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using ClientCommon.Data.Config;
using ClientCommon.Data.InformationContext;

namespace ClientCommon.Installer.Utilities
{
    public class Configuration
    {
        public static async Task InstallFiles(Assembly installerAssembly, string targetDirectory, Hashtable fileMappings = default, Regex resourceRegex = null, CancellationToken cancellationToken = default)
        {
            var resources = installerAssembly.GetManifestResourceNames();
            resourceRegex ??= new Regex(@"Clients\..*\.Installer\.Resources\.(.*)");


            foreach (string resourceName in resources.ToList())
            {
                if (resourceRegex.IsMatch(resourceName))
                {
                    var matches = resourceRegex.Matches(resourceName);
                    var fileName = matches[0].Groups[1].Value;

                    var fileDestinationPath = fileMappings.ContainsKey(resourceName)
                        ? (string)fileMappings[resourceName]
                        : Path.Join(targetDirectory, fileName);

                    var stream = installerAssembly.GetManifestResourceStream(resourceName);
                    var fileDestination = File.OpenWrite(fileDestinationPath);

                    // Why must it be done this way?
                    // I don't know. But if you remove the .Wait() call, the program mysteriously ends with error code 0
                    // and no exceptions thrown
                    stream.CopyToAsync(fileDestination, cancellationToken).Wait(cancellationToken);
                    await stream.DisposeAsync();
                    await fileDestination.DisposeAsync();
                }
            }

            return;
        }

        public static async Task<ServiceConfiguration> Configure(IConfigurationManager configurationManager, IClientInformationContext informationContext, int? inputSystemId, int? inputTeamId, string inputControllerHost, CancellationToken cancellationToken = default)
        {
            var config = await configurationManager.LoadConfiguration(cancellationToken);

            string controllerHost = await GetServerHost(informationContext, inputControllerHost, cancellationToken);

            config.EngineControllerHost = controllerHost;

            await configurationManager.SaveConfiguration(config, cancellationToken);

            await informationContext.ConnectAsync();

            config.SystemIdentifier = await GetSystemId(informationContext, inputSystemId, cancellationToken);
            config.TeamID = await GetTeamId(informationContext, inputTeamId, cancellationToken);

            await configurationManager.SaveConfiguration(config, cancellationToken);

            if (config.TeamID is int teamId)
            {
                await informationContext.RegisterVM(teamId, cancellationToken);
            }

            return config;
        }

        private static async Task<int> GetSystemId(IClientInformationContext informationContext, int? inputSystemId, CancellationToken token = default)
        {
            if (inputSystemId == 0 || inputSystemId is null)
            {
                var systems = await informationContext.GetAvailableSystemsAsync(token);

                Console.WriteLine("Available systems:");
                foreach (var system in systems)
                {
                    Console.WriteLine($"ID: {system.ID}; Name: {system.SystemIdentifier}");
                }
                Console.Write("\n");

                while (inputSystemId is null || inputSystemId == 0)
                {
                    Console.WriteLine("Enter a system to configure this system to use (either the ID or name):");
                    Console.Write(" > ");
                    string input = Console.ReadLine();

                    _ = int.TryParse(input, out int result);

                    var targetSystem = systems.First(sys => sys.ID == result || sys.SystemIdentifier == input.Trim());

                    if (targetSystem != null)
                    {
                        inputSystemId = targetSystem.ID;
                    }
                }
            }

            return (int)inputSystemId;
        }

        private static async Task<string> GetServerHost(IClientInformationContext informationContext, string inputServerHost, CancellationToken token = default)
        {
            while (inputServerHost is null || !await informationContext.TestConnectionAsync(inputServerHost, cancellationToken: token))
            {
                Console.WriteLine("Enter the host of the engine controller");
                Console.WriteLine(" > ");

                inputServerHost = Console.ReadLine().Trim();
            }

            return inputServerHost;
        }

        private static async Task<int?> GetTeamId(IClientInformationContext informationContext, int? inputTeamId, CancellationToken cancellationToken = default)
        {
            if (inputTeamId is null)
            {
                return null;
            }

            var teams = await informationContext.GetTeamsAsync(cancellationToken);

            if (teams.First(t => t.ID == inputTeamId) != null)
            {
                return inputTeamId;
            }

            Console.WriteLine("Available teams:");

            foreach (var team in teams)
            {
                Console.WriteLine($"{team.Name}; ID {team.ID}");
            }

            while (teams.First(t => t.ID == inputTeamId) == null)
            {
                Console.WriteLine("Team ID provided does not match a known team. Please enter a valid team ID:");
                Console.Write(" > ");

                string input = Console.ReadLine();

                _ = int.TryParse(input, out int result);
                inputTeamId = result;
            }

            return inputTeamId;
        }
    }
}
