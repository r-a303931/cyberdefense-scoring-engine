#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Common.Models;

namespace ClientCommon.Data.InformationContext
{
    public class TestClientInformationContext : IClientInformationContext
    {
        public Task ConnectAsync() => Task.CompletedTask;

        private Team? Team { get; set; }
        private CompetitionSystem? CompetitionSystem { get; set; }
        

        public async Task SetTeamIdAsync(int id, CancellationToken cancellationToken = default)
        {
            Team = (await GetTeamsAsync(cancellationToken)).First(t => t.ID == id);
        }

        public Task<int?> GetTeamIdAsync(CancellationToken cancellationToken = default) => Task.FromResult(Team?.ID);

        public Task<Team?> GetTeam(CancellationToken cancellationToken = default) => Task.FromResult(Team);

        public async Task<IEnumerable<Team>> GetTeamsAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new Team[]
            {
                new Team
                {
                    ID = 1,
                    Name = "Alpha",
                    AppliedCompetitionPenalties = { },
                    CompletedCompetitionTasks = { }
                }
            }.ToList());
        }

        public async Task SetSystemIdentifierAsync(int SystemIdentifier, CancellationToken cancellationToken = default)
        {
            CompetitionSystem = (await GetAvailableSystemsAsync(cancellationToken)).First(t => t.ID == SystemIdentifier);
        }

        public Task<int?> GetSystemIdentifierAsync(CancellationToken cancellationToken = default) => Task.FromResult(CompetitionSystem?.ID);

        public Task<CompetitionSystem?> GetSystem(CancellationToken cancellationToken = default) => Task.FromResult(CompetitionSystem);

        public async Task<IEnumerable<CompetitionSystem>> GetAvailableSystemsAsync(CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new CompetitionSystem[]
            {
                new CompetitionSystem
                {
                    CompetitionPenalties = { },
                    CompetitionTasks = { },
                    ID = 1,
                    ReadmeText = "",
                    SystemIdentifier = "Windows 10"
                }
            }.ToList());
        }

        public Task SetCompletedTasksAsync(IEnumerable<CompetitionTask> tasks, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task SetAppliedPenaltiesAsync(IEnumerable<CompetitionPenalty> penalties, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task RegisterVM(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RegisterVM(int teamID, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SignIn(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<bool> TestConnectionAsync(bool verbose = true, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> TestConnectionAsync(string host, bool verbose = true, CancellationToken cancellationToken = default) => Task.FromResult(true);

    }
}
