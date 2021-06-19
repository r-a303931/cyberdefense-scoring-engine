#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using ClientCommon.Models;

namespace ClientCommon.Data
{
    public class ClientInformationContext : IClientInformationContext
    {
        public Config.IConfigurationManager ConfigurationManager { get; init; }
        public HttpClient HttpClient { get; init; } = new HttpClient();

        public ClientInformationContext(Config.IConfigurationManager configurationManager)
        {
            ConfigurationManager = configurationManager;
        }

        public async Task<IEnumerable<CompetitionSystem>> GetAvailableSystemsAsync(CancellationToken cancellationToken = default)
        {
            var config = await ConfigurationManager.LoadConfiguration(cancellationToken);

            if (config.EngineControllerHost is null)
            {
                throw new Exception("Engine controller host is undefined");
            }

            using var httpResponse = await HttpClient.GetAsync($"http://{config.EngineControllerHost}:5000/api/systems", cancellationToken);

            httpResponse.EnsureSuccessStatusCode();

            using var contentStream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<IEnumerable<CompetitionSystem>>(contentStream, new JsonSerializerOptions { }, cancellationToken) switch
            {
                null => throw new Exception("Invalid system list"),
                var list => list
            };
        }

        public async Task SetSystemIdentifierAsync(int SystemIdentifier, CancellationToken cancellationToken = default)
        {
            var config = await ConfigurationManager.LoadConfiguration(cancellationToken);

            config.SystemIdentifier = SystemIdentifier;

            await ConfigurationManager.SaveConfiguration(config, cancellationToken);
        }

        public async Task<int?> GetSystemIdentifierAsync(CancellationToken cancellationToken = default) =>
            (await ConfigurationManager.LoadConfiguration(cancellationToken))?.SystemIdentifier;

        public async Task<IEnumerable<Team>> GetTeamsAsync(CancellationToken cancellationToken = default)
        {
            var config = await ConfigurationManager.LoadConfiguration(cancellationToken);

            if (config.EngineControllerHost is null)
            {
                throw new Exception("Engine controller host is undefined");
            }

            using var httpResponse = await HttpClient.GetAsync($"http://{config.EngineControllerHost}:5000/api/teams", cancellationToken);

            httpResponse.EnsureSuccessStatusCode();

            using var contentStream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync<IEnumerable<Team>>(contentStream, new JsonSerializerOptions { }, cancellationToken) switch
            {
                null => throw new Exception("Invalid team list"),
                var list => list
            };
        }

        public async Task SetTeamIdAsync(int TeamID, CancellationToken cancellationToken = default)
        {
            var config = await ConfigurationManager.LoadConfiguration(cancellationToken);

            config.TeamID = TeamID;

            await ConfigurationManager.SaveConfiguration(config, cancellationToken);
        }

        public async Task<int?> GetTeamIdAsync(CancellationToken cancellationToken = default) =>
            (await ConfigurationManager.LoadConfiguration(cancellationToken))?.TeamID;

        public async Task SetCompletedTasksAsync(IEnumerable<CompetitionTask> tasks, CancellationToken cancellationToken = default)
        {
            var config = await ConfigurationManager.LoadConfiguration(cancellationToken);

            if (config.EngineControllerHost is null)
            {
                throw new Exception("Engine controller host is undefined");
            }

            var teamID = await GetTeamIdAsync(cancellationToken);

            if (teamID is null) {
                throw new Exception("Service is not configured with team");
            }

            var content = JsonContent.Create(from task in tasks select task.ID);

            using var httpResponse = await HttpClient.PostAsync($"http://{config.EngineControllerHost}:5000/api/teams/completetask?teamID={teamID}", content, cancellationToken);

            httpResponse.EnsureSuccessStatusCode();
        }

        public async Task SetAppliedPenaltiesAsync(IEnumerable<CompetitionPenalty> penalties, CancellationToken cancellationToken = default)
        {
            var config = await ConfigurationManager.LoadConfiguration(cancellationToken);

            if (config.EngineControllerHost is null)
            {
                throw new Exception("Engine controller host is undefined");
            }

            var teamID = await GetTeamIdAsync(cancellationToken);

            if (teamID is null)
            {
                throw new Exception("Service is not configured with team");
            }

            var content = JsonContent.Create(from penalty in penalties select penalty.ID);

            using var httpResponse = await HttpClient.PostAsync($"http://{config.EngineControllerHost}:5000/api/teams/applypenalty?teamID={teamID}", content, cancellationToken);

            httpResponse.EnsureSuccessStatusCode();
        }

        public async Task<bool> TestConnectionAsync(bool verbose = false, CancellationToken cancellationToken = default)
        {
            var host = (await ConfigurationManager.LoadConfiguration(cancellationToken)).EngineControllerHost;

            if (host is null)
            {
                throw new Exception("Invalid server host provided");
            }

            return await TestConnectionAsync(host, verbose, cancellationToken);
        }

        public async Task<bool> TestConnectionAsync(string host, bool verbose = false, CancellationToken cancellationToken = default)
        {
            try
            {
                using var httpResponse = await HttpClient.GetAsync($"http://{host}:5000/api/connection", cancellationToken);

                httpResponse.EnsureSuccessStatusCode();

                using var contentStream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
                return await JsonSerializer.DeserializeAsync<ConnectionTestResult>(contentStream, new JsonSerializerOptions { }, cancellationToken) switch
                {
                    null => false,
                    var res => res.Success
                };
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Connection failed. Details:");
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
                
                return false;
            }
        }
    }

    class ConnectionTestResult
    {
        public bool Success { get; set; }
    }
}
