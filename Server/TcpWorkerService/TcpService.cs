using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Common;
using Common.Models;
using Common.Protocol;
using Common.Protocol.CompetitionMessages;
using EngineController.Data;

namespace EngineController.Workers.TcpConnectionService
{

    public class TcpService : BackgroundService
    {
        private readonly ILogger<TcpService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public TcpService(ILogger<TcpService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var server = TcpListener.Create(Constants.TcpControlPort);
            server.Start();

            _logger.LogInformation("Started listening for new clients");

            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await Task.Run(() => server.AcceptTcpClientAsync(), cancellationToken);

                _logger.LogInformation("Found new client connection");

                HandleClient(client, cancellationToken);
            }
        }

        private async void HandleClient(TcpClient _client, CancellationToken cancellationToken = default)
        {
            // Make it easier to dispose of the TcpClient
            using var client = _client;
            using var protocol = new JsonCompetitionProtocol(client, _logger);

            int teamID;
            int SystemIdentifier;

            var loginMessage = await protocol.GetMessageAsync<Login>(cancellationToken: cancellationToken);

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<EngineControllerContext>();

                var teamVmRegistration = await context.RegisteredVirtualMachines.FirstOrDefaultAsync(rvm => rvm.Id == loginMessage.VmId, cancellationToken);

                if (teamVmRegistration is null)
                {
                    await protocol.SendMessage(new CompetitionError
                    {
                        ResponseGuid = loginMessage.ResponseGuid,
                        ErrorMessage = "Could not find VM registration"
                    });
                }

                teamID = teamVmRegistration.TeamID;
                SystemIdentifier = teamVmRegistration.SystemIdentifier;

                teamVmRegistration.IsConnectedNow = true;
                await context.SaveChangesAsync(cancellationToken);
            }

            try
            {
                protocol.AddMessageHandler<Requests.GetCompetitionSystems>(async (source, msg) =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetService<EngineControllerContext>();

                    try
                    {
                        var systems = await context.CompetitionSystems.ToListAsync();

                        await protocol.SendMessage(new CompetitionSystemsList
                        {
                            ResponseGuid = msg.ResponseGuid,
                            CompetitionSystems = from competitionSystem
                                                 in systems
                                                 select new CompetitionSystem
                                                 {
                                                     CompetitionPenalties = from penalty
                                                                            in competitionSystem.CompetitionPenalties
                                                                            select new CompetitionPenalty
                                                                            {
                                                                                ID = penalty.ID,
                                                                                Points = penalty.Points,
                                                                                ScriptType = (ScriptType)penalty.ScriptType,
                                                                                SystemIdentifier = penalty.SystemIdentifier,
                                                                                PenaltyName = penalty.PenaltyName,
                                                                                PenaltyScript = penalty.PenaltyScript
                                                                            },
                                                     CompetitionTasks = from task
                                                                        in competitionSystem.CompetitionTasks
                                                                        select new CompetitionTask
                                                                        {
                                                                            ID = task.ID,
                                                                            Points = task.Points,
                                                                            ScriptType = (ScriptType)task.ScriptType,
                                                                            SystemIdentifier = task.SystemIdentifier,
                                                                            TaskName = task.TaskName,
                                                                            ValidationScript = task.ValidationScript
                                                                        },
                                                     ID = competitionSystem.ID,
                                                     ReadmeText = competitionSystem.ReadmeText,
                                                     SystemIdentifier = competitionSystem.SystemIdentifier
                                                 }
                        });
                    }
                    catch (Exception e)
                    {
                        await protocol.SendMessage(new CompetitionError
                        {
                            ResponseGuid = msg.ResponseGuid,
                            ErrorMessage = $"Could not get systems: {e.Message}"
                        });

                        _logger.LogError($"Could not get systems: {e.Message}\n{e.StackTrace}");
                    }
                });

                protocol.AddMessageHandler<Requests.GetTeams>(async (source, msg) =>
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var context = scope.ServiceProvider.GetService<EngineControllerContext>();

                        var teams = await context.Teams
                            .Include(t => t.AppliedCompetitionPenalties)
                            .ThenInclude(p => p.CompetitionPenalty)
                            .Include(t => t.CompletedCompetitionTasks)
                            .ThenInclude(t => t.CompetitionTask)
                            .ToListAsync();

                        await protocol.SendMessage(new TeamsList
                        {
                            ResponseGuid = msg.ResponseGuid,
                            Teams = from team
                                    in teams
                                    select new Team
                                    {
                                        AppliedCompetitionPenalties = from penalty
                                                                      in team.AppliedCompetitionPenalties
                                                                      select new AppliedCompetitionPenalty
                                                                      {
                                                                          CompetitionPenalty = new CompetitionPenalty
                                                                          {
                                                                              ID = penalty.CompetitionPenalty.ID,
                                                                              PenaltyName = penalty.CompetitionPenalty.PenaltyName,
                                                                              PenaltyScript = penalty.CompetitionPenalty.PenaltyScript,
                                                                              Points = penalty.CompetitionPenalty.Points,
                                                                              ScriptType = (ScriptType)penalty.CompetitionPenalty.ScriptType,
                                                                              SystemIdentifier = penalty.CompetitionPenalty.SystemIdentifier
                                                                          },
                                                                          CompetitionPenaltyID = penalty.CompetitionPenaltyID,
                                                                          TeamID = team.ID
                                                                      },
                                        CompletedCompetitionTasks = from task
                                                                    in team.CompletedCompetitionTasks
                                                                    select new CompletedCompetitionTask
                                                                    {
                                                                        CompetitionTask = new CompetitionTask
                                                                        {
                                                                            ID = task.CompetitionTaskID,
                                                                            Points = task.CompetitionTask.Points,
                                                                            ScriptType = (ScriptType)task.CompetitionTask.ScriptType,
                                                                            SystemIdentifier = task.CompetitionTask.SystemIdentifier,
                                                                            TaskName = task.CompetitionTask.TaskName,
                                                                            ValidationScript = task.CompetitionTask.ValidationScript
                                                                        },
                                                                        CompetitionTaskID = task.CompetitionTaskID,
                                                                        TeamID = team.ID
                                                                    },
                                        ID = team.ID,
                                        Name = team.Name
                                    }
                        });
                    }
                    catch (Exception e)
                    {
                        await protocol.SendMessage(new CompetitionError
                        {
                            ResponseGuid = msg.ResponseGuid,
                            ErrorMessage = $"Could not get teams: {e.Message}"
                        });

                        _logger.LogError($"Could not get teams: {e.Message}\n{e.StackTrace}");
                    }
                });

                protocol.AddMessageHandler<SetCompletedTasks>(async (source, msg) =>
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var context = scope.ServiceProvider.GetService<EngineControllerContext>();

                        var taskIDs = msg.TaskIds;

                        var team = await context.Teams
                            .Include(t => t.CompletedCompetitionTasks)
                            .ThenInclude(t => t.CompetitionTask)
                            .FirstOrDefaultAsync(team => team.ID == teamID);

                        IEnumerable<int> collectionTaskIDs = from task
                                                             in team.CompletedCompetitionTasks
                                                             where task.CompetitionTask.SystemIdentifier == SystemIdentifier
                                                             select task.CompetitionTaskID;

                        IEnumerable<int> taskIDsToBeAdded = taskIDs.Except(collectionTaskIDs);
                        IEnumerable<int> taskIDsToBeRemoved = collectionTaskIDs.Except(taskIDs);

                        foreach (var taskIDToAdd in taskIDsToBeAdded)
                        {
                            team.CompletedCompetitionTasks.Add(new Models.CompletedCompetitionTask
                            {
                                TeamID = team.ID,
                                CompetitionTaskID = taskIDToAdd
                            });
                        }

                        foreach (var taskIDToRemove in taskIDsToBeRemoved)
                        {
                            team.CompletedCompetitionTasks.Remove(new Models.CompletedCompetitionTask
                            {
                                TeamID = team.ID,
                                CompetitionTaskID = taskIDToRemove
                            });
                        }

                        await context.SaveChangesAsync();

                        await protocol.SendMessage(new CommandAcknowledge
                        {
                            ResponseGuid = msg.ResponseGuid
                        });
                    }
                    catch (Exception e)
                    {
                        await protocol.SendMessage(new CompetitionError
                        {
                            ResponseGuid = msg.ResponseGuid,
                            ErrorMessage = $"Could not set completed tasks: {e.Message}"
                        });

                        _logger.LogError($"Could not set completed tasks: {e.Message}\n{e.StackTrace}");
                    }
                });

                protocol.AddMessageHandler<SetAppliedPenalties>(async (source, msg) =>
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var context = scope.ServiceProvider.GetService<EngineControllerContext>();

                        var penaltyIDs = msg.PenaltyIds;

                        var team = await context.Teams
                            .Include(t => t.AppliedCompetitionPenalties)
                            .FirstOrDefaultAsync(team => team.ID == teamID);

                        IEnumerable<int> competitionPenaltyIDs = from penalty
                                                                 in team.AppliedCompetitionPenalties
                                                                 where penalty.CompetitionPenalty.SystemIdentifier == SystemIdentifier
                                                                 select penalty.CompetitionPenaltyID;

                        IEnumerable<int> taskIDsToBeAdded = penaltyIDs.Except(competitionPenaltyIDs);
                        IEnumerable<int> taskIDsToBeRemoved = competitionPenaltyIDs.Except(penaltyIDs);

                        foreach (var penaltyIDToAdd in taskIDsToBeAdded)
                        {
                            team.AppliedCompetitionPenalties.Add(new Models.AppliedCompetitionPenalty
                            {
                                TeamID = team.ID,
                                CompetitionPenaltyID = penaltyIDToAdd
                            });
                        }

                        foreach (var penaltyIDToRemove in taskIDsToBeRemoved)
                        {
                            team.AppliedCompetitionPenalties.Remove(new Models.AppliedCompetitionPenalty
                            {
                                TeamID = team.ID,
                                CompetitionPenaltyID = penaltyIDToRemove
                            });
                        }

                        await context.SaveChangesAsync();

                        await protocol.SendMessage(new CommandAcknowledge
                        {
                            ResponseGuid = msg.ResponseGuid
                        });
                    }
                    catch (Exception e)
                    {
                        await protocol.SendMessage(new CompetitionError
                        {
                            ResponseGuid = msg.ResponseGuid,
                            ErrorMessage = $"Could not set penalties: {e.Message}"
                        });

                        _logger.LogError($"Could not set penalties: {e.Message}\n{e.StackTrace}");
                    }
                });

                await protocol.GetSessionCompletionTask(cancellationToken);
            }
            catch (Exception e)
            {
                await protocol.SendMessage(new CompetitionError
                {
                    ResponseGuid = loginMessage.ResponseGuid,
                    ErrorMessage = $"Could not login: {e.Message}"
                });

                _logger.LogError($"Could not login: {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetService<EngineControllerContext>();

                var teamVmRegistration = await context.RegisteredVirtualMachines.FirstOrDefaultAsync(rvm => rvm.Id == loginMessage.VmId, cancellationToken);
                teamVmRegistration.IsConnectedNow = false;
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}