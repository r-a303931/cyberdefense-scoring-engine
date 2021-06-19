#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ClientCommon.Models;

namespace ClientCommon.Data
{
    public interface IClientInformationContext
    {
        Task<IEnumerable<CompetitionSystem>> GetAvailableSystemsAsync(CancellationToken cancellationToken = default);
        Task SetSystemIdentifierAsync(int SystemIdentifier, CancellationToken cancellationToken = default);
        Task<int?> GetSystemIdentifierAsync(CancellationToken cancellationToken = default);
        async Task<CompetitionSystem?> GetSystem(CancellationToken cancellationToken = default) =>
            await GetSystemIdentifierAsync(cancellationToken) switch
            {
                null => null,
                int ID => (await GetAvailableSystemsAsync(cancellationToken)).First(t => t.ID == ID)
            };

        Task<IEnumerable<Team>> GetTeamsAsync(CancellationToken cancellationToken = default);
        Task SetTeamIdAsync(int ID, CancellationToken cancellationToken = default);
        Task<int?> GetTeamIdAsync(CancellationToken cancellationToken = default);
        async Task<Team?> GetTeam(CancellationToken cancellationToken = default) =>
            await GetTeamIdAsync(cancellationToken) switch
            {
                null => null,
                int ID => (await GetTeamsAsync(cancellationToken)).First(t => t.ID == ID)
            };


        Task SetCompletedTasksAsync(IEnumerable<CompetitionTask> TaskIds, CancellationToken cancellationToken = default);

        Task SetAppliedPenaltiesAsync(IEnumerable<CompetitionPenalty> PenaltyIds, CancellationToken cancellationToken = default);

        Task<bool> TestConnectionAsync(bool verbose = true, CancellationToken cancellationToken = default);
        Task<bool> TestConnectionAsync(string serverHost, bool verbose = true, CancellationToken cancellationToken = default);
    }
}
