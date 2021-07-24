#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Common;
using Common.Models;
using Common.Protocol;
using Common.Protocol.CompetitionMessages;

namespace ClientCommon.Data.InformationContext
{
    public class TcpClientInformationClientContext : IClientInformationContext
    {
        public Config.IConfigurationManager ConfigurationManager { get; private set; }
        public JsonCompetitionProtocol CompetitionProtocol { get; private set; }

        public TcpClientInformationClientContext(Config.IConfigurationManager configurationManager, string host)
            : this(configurationManager)
        {
            var client = new TcpClient(host, Constants.TcpControlPort);
            CompetitionProtocol = new JsonCompetitionProtocol(client);

            var systemReadyMessageReceivedTask = CompetitionProtocol.GetMessageAsync<SystemReady>();

            CompetitionProtocol.StartConnection();

            Console.WriteLine("Waiting for system ready...");
            systemReadyMessageReceivedTask.Wait();
            Console.WriteLine("System ready.");
        }

        public TcpClientInformationClientContext(Config.IConfigurationManager configurationManager)
        {
            ConfigurationManager = configurationManager;
        }

        public async Task ConnectAsync()
        {
            if (CompetitionProtocol != null)
            {
                throw new Exception("Already connected");
            }

            var config = await ConfigurationManager.LoadConfiguration();

            if (config.EngineControllerHost is string host)
            {
                var client = new TcpClient(host, Constants.TcpControlPort);
                CompetitionProtocol = new JsonCompetitionProtocol(client);
                CompetitionProtocol.StartConnection();

                Console.WriteLine("Waiting for system ready...");
                CompetitionProtocol.GetMessageAsync<SystemReady>().Wait();
                Console.WriteLine("System ready.");
            }
            else
            {
                throw new Exception("Client is not configured with an engine controller host");
            }
        }

        public async Task<IEnumerable<CompetitionSystem>> GetAvailableSystemsAsync(CancellationToken cancellationToken = default) =>
            (await CompetitionProtocol.SendMessage<Requests.GetCompetitionSystems, CompetitionSystemsList>(new Requests.GetCompetitionSystems { }, cancellationToken: cancellationToken)).CompetitionSystems;

        public async Task SetSystemIdentifierAsync(int SystemIdentifier, CancellationToken cancellationToken = default)
        {
            var config = await ConfigurationManager.LoadConfiguration(cancellationToken);

            config.SystemIdentifier = SystemIdentifier;

            await ConfigurationManager.SaveConfiguration(config, cancellationToken);
        }

        public async Task<int?> GetSystemIdentifierAsync(CancellationToken cancellationToken = default) =>
            (await ConfigurationManager.LoadConfiguration(cancellationToken))?.SystemIdentifier;

        public async Task<IEnumerable<Team>> GetTeamsAsync(CancellationToken cancellationToken = default) =>
            (await CompetitionProtocol.SendMessage<Requests.GetTeams, TeamsList>(new Requests.GetTeams { }, cancellationToken: cancellationToken)).Teams;

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
            var teamIdMaybe = await GetTeamIdAsync(cancellationToken);

            if (teamIdMaybe is int teamId)
            {
                var msg = await CompetitionProtocol.SendVoidMessage(new SetCompletedTasks
                {
                    TaskIds = from task
                              in tasks
                              select task.ID
                }, cancellationToken: cancellationToken);
            }
            else
            {
                throw new Exception("Team is not set up yet, cannot apply task IDs");
            }
        }

        public async Task SetAppliedPenaltiesAsync(IEnumerable<CompetitionPenalty> penalties, CancellationToken cancellationToken = default)
        {
            var teamIdMaybe = await GetTeamIdAsync(cancellationToken);

            if (teamIdMaybe is int teamId)
            {
                var msg = await CompetitionProtocol.SendVoidMessage(new SetAppliedPenalties
                {
                    PenaltyIds = from penalty
                                 in penalties
                                 select penalty.ID
                }, cancellationToken: cancellationToken);
            }
            else
            {
                throw new Exception("Team is not set up yet, cannot apply task IDs");
            }
        }

        public async Task RegisterVM(int teamID, CancellationToken cancellationToken = default)
        {
            await SetTeamIdAsync(teamID, cancellationToken);

            var config = await ConfigurationManager.LoadConfiguration(cancellationToken);

            if (config.SystemIdentifier is int sysId)
            {
                await CompetitionProtocol.SendVoidMessage(new RegisterVM
                {
                    Id = config.SystemGUID,
                    SystemIdentifier = sysId,
                    TeamId = teamID
                }, cancellationToken: cancellationToken);
            }
            else
            {
                throw new Exception("System is not fully configured; SystemIdentifier is null");
            }
        }

        public async Task RegisterVM(CancellationToken cancellationToken = default)
        {
            var config = await ConfigurationManager.LoadConfiguration(cancellationToken);

            if (config.SystemIdentifier is int sysId && config.TeamID is int teamID)
            {
                await CompetitionProtocol.SendVoidMessage(new RegisterVM
                {
                    Id = config.SystemGUID,
                    SystemIdentifier = sysId,
                    TeamId = teamID
                }, cancellationToken: cancellationToken);
            }
            else
            {
                throw new Exception("System is not fully configured; SystemIdentifier or TeamID is null");
            }
        }

        public async Task SignIn(CancellationToken cancellationToken = default)
        {
            var config = await ConfigurationManager.LoadConfiguration(cancellationToken);

            await CompetitionProtocol.SendVoidMessage(new Login
            {
                VmId = config.SystemGUID
            }, cancellationToken: cancellationToken);
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
            using var client = new TcpClient(host, Constants.TcpControlPort);
            using var protocol = new JsonCompetitionProtocol(client);
            protocol.StartConnection();

            await protocol.SendHeartbeat(cancellationToken);

            return true;
        }
    }
}
