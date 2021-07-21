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
            CompetitionProtocol.StartConnection();
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
            }
            else
            {
                throw new Exception("Client is not configured with an engine controller host");
            }
        }

        public async Task<IEnumerable<CompetitionSystem>> GetAvailableSystemsAsync(CancellationToken cancellationToken = default)
        {
            var msg = await CompetitionProtocol.SendMessage(new Requests.GetCompetitionSystems { }, cancellationToken);
            var result = await CompetitionProtocol.GetResponseAsync<CompetitionSystemsList, Requests.GetCompetitionSystems>(msg, cancellationToken: cancellationToken);

            return result.CompetitionSystems;
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
            var msg = await CompetitionProtocol.SendMessage(new Requests.GetTeams { }, cancellationToken);
            var result = await CompetitionProtocol.GetResponseAsync<TeamsList, Requests.GetTeams>(msg, cancellationToken: cancellationToken);

            return result.Teams;
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
            var teamIdMaybe = await GetTeamIdAsync(cancellationToken);

            if (teamIdMaybe is int teamId)
            {
                var msg = await CompetitionProtocol.SendMessage(new SetCompletedTasks
                {
                    TaskIds = from task
                              in tasks
                              select task.ID
                }, cancellationToken);

                await CompetitionProtocol.GetAcknowledgementAsync(msg, cancellationToken: cancellationToken);
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
                var msg = await CompetitionProtocol.SendMessage(new SetAppliedPenalties
                {
                    PenaltyIds = from penalty
                                 in penalties
                                 select penalty.ID
                }, cancellationToken);

                await CompetitionProtocol.GetAcknowledgementAsync(msg, cancellationToken: cancellationToken);
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
                var msg = await CompetitionProtocol.SendMessage(new RegisterVM
                {
                    Id = config.SystemGUID,
                    SystemIdentifier = sysId,
                    TeamId = teamID
                }, cancellationToken);

                await CompetitionProtocol.GetAcknowledgementAsync(msg, cancellationToken: cancellationToken);
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
                var msg = await CompetitionProtocol.SendMessage(new RegisterVM
                {
                    Id = config.SystemGUID,
                    SystemIdentifier = sysId,
                    TeamId = teamID
                }, cancellationToken);

                await CompetitionProtocol.GetAcknowledgementAsync(msg, cancellationToken: cancellationToken);
            }
            else
            {
                throw new Exception("System is not fully configured; SystemIdentifier or TeamID is null");
            }
        }

        public async Task SignIn(CancellationToken cancellationToken = default)
        {
            var config = await ConfigurationManager.LoadConfiguration(cancellationToken);

            var msg = await CompetitionProtocol.SendMessage(new Login
            {
                VmId = config.SystemGUID
            }, cancellationToken);

            await CompetitionProtocol.GetAcknowledgementAsync(msg, cancellationToken: cancellationToken);
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

            await protocol.DisposeAsync();

            return true;
        }
    }
}
