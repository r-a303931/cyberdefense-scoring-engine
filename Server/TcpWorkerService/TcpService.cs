using System;
using System.Collections.Generic;
using System.Linq;
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

        private async void HandleClient(TcpClient client, CancellationToken cancellationToken = default)
        {
            // Make it easier to dispose of the TcpClient
            using var protocol = new JsonCompetitionProtocol(client, _logger);
            protocol.StartConnection();

            protocol.AddMessageHandler<Requests.GetCompetitionSystems>(async (source, msg) =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetService<EngineControllerContext>();

                    var systems = await context.CompetitionSystems.ToListAsync();

                    var message = new CompetitionSystemsList
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
                    };

                    await protocol.SendMessage(message);
                }
                catch (Exception e)
                {
                    await protocol.SendMessage(new CompetitionError
                    {
                        ResponseGuid = msg.ResponseGuid,
                        ErrorMessage = $"Could not get systems: {e.Message}"
                    });

                    _logger.LogError(e, "Could not get systems");
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

                    _logger.LogError(e, $"Could not get teams");
                }
            });

            // These are assigned later on
            int teamID = 0;
            int SystemIdentifier = 0;
            Guid VmId = default;

            CompetitionMessage firstMessage;
            Models.RegisteredVirtualMachine teamVmRegistration = null;

            try
            {
                firstMessage = await await Task.WhenAny(
                    protocol.GetMessageAsync<Login>(-1, cancellationToken: cancellationToken)
                        .ContinueWith(lm => (CompetitionMessage)lm.Result),
                    protocol.GetMessageAsync<RegisterVM>(-1, cancellationToken: cancellationToken)
                        .ContinueWith(lm => (CompetitionMessage)lm.Result)
                );

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetService<EngineControllerContext>();

                if (firstMessage is Login loginMessage)
                {
                    teamVmRegistration = await context.RegisteredVirtualMachines.FirstOrDefaultAsync(rvm => rvm.VmId == loginMessage.VmId, cancellationToken);

                    if (teamVmRegistration is null)
                    {
                        await protocol.SendMessage(new CompetitionError
                        {
                            ResponseGuid = loginMessage.ResponseGuid,
                            ErrorMessage = "Could not find VM registration"
                        }, cancellationToken);

                        return;
                    }

                    teamID = teamVmRegistration.TeamID;
                    SystemIdentifier = teamVmRegistration.SystemIdentifier;
                    VmId = teamVmRegistration.VmId;

                    teamVmRegistration.IsConnectedNow = true;
                    await context.SaveChangesAsync(cancellationToken);

                    await protocol.SendMessage(new CommandAcknowledge
                    {
                        ResponseGuid = loginMessage.ResponseGuid
                    }, cancellationToken);
                }
                else if (firstMessage is RegisterVM rvm)
                {
                    teamVmRegistration = new Models.RegisteredVirtualMachine
                    {
                        VmId = rvm.Id,
                        SystemIdentifier = rvm.SystemIdentifier,
                        IsConnectedNow = true,
                        LastCheckIn = DateTime.Now,
                        TeamID = rvm.TeamId
                    };

                    teamID = rvm.TeamId;
                    SystemIdentifier = rvm.SystemIdentifier;
                    VmId = rvm.Id;

                    try
                    {
                        await context.RegisteredVirtualMachines.AddAsync(teamVmRegistration, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);

                        await protocol.SendMessage(new CommandAcknowledge
                        {
                            ResponseGuid = rvm.ResponseGuid
                        }, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        await protocol.SendMessage(new CompetitionError
                        {
                            ResponseGuid = rvm.ResponseGuid,
                            ErrorMessage = "Could not register VM: " + e.Message
                        }, cancellationToken);
                        _logger.LogError(e, "Could not register VM");

                        return;
                    }
                }
                else
                {
                    await protocol.SendMessage(new CompetitionError
                    {
                        ResponseGuid = firstMessage.ResponseGuid,
                        ErrorMessage = "Invalid message sent"
                    }, cancellationToken);
                    return;
                }
            }
            catch (AggregateException e)
            {
                e.Handle(ex => true);
                _logger.LogError(e, "Could not login VM before closing connection");
                return;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not login VM before closing connection");
                return;
            }

            try
            {
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
                                                                && task.VmId == VmId
                                                             select task.CompetitionTaskID;

                        IEnumerable<int> taskIDsToBeAdded = taskIDs.Except(collectionTaskIDs);
                        IEnumerable<int> taskIDsToBeRemoved = collectionTaskIDs.Except(taskIDs);

                        foreach (var taskIDToAdd in taskIDsToBeAdded)
                        {
                            team.CompletedCompetitionTasks.Add(new Models.CompletedCompetitionTask
                            {
                                TeamID = team.ID,
                                CompetitionTaskID = taskIDToAdd,
                                VmId = VmId
                            });
                        }

                        foreach (var taskIDToRemove in taskIDsToBeRemoved)
                        {
                            team.CompletedCompetitionTasks.Remove(new Models.CompletedCompetitionTask
                            {
                                TeamID = team.ID,
                                CompetitionTaskID = taskIDToRemove,
                                VmId = VmId
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

                        _logger.LogError(e, $"Could not set completed tasks");
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
                            .ThenInclude(p => p.CompetitionPenalty)
                            .FirstOrDefaultAsync(team => team.ID == teamID);

                        IEnumerable<int> competitionPenaltyIDs = from penalty
                                                                 in team.AppliedCompetitionPenalties
                                                                 where penalty.CompetitionPenalty.SystemIdentifier == SystemIdentifier
                                                                    && penalty.VmId == VmId
                                                                 select penalty.CompetitionPenaltyID;

                        IEnumerable<int> taskIDsToBeAdded = penaltyIDs.Except(competitionPenaltyIDs);
                        IEnumerable<int> taskIDsToBeRemoved = competitionPenaltyIDs.Except(penaltyIDs);

                        foreach (var penaltyIDToAdd in taskIDsToBeAdded)
                        {
                            team.AppliedCompetitionPenalties.Add(new Models.AppliedCompetitionPenalty
                            {
                                TeamID = team.ID,
                                CompetitionPenaltyID = penaltyIDToAdd,
                                VmId = VmId
                            });
                        }

                        foreach (var penaltyIDToRemove in taskIDsToBeRemoved)
                        {
                            team.AppliedCompetitionPenalties.Remove(new Models.AppliedCompetitionPenalty
                            {
                                TeamID = team.ID,
                                CompetitionPenaltyID = penaltyIDToRemove,
                                VmId = VmId
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

                        _logger.LogError(e, $"Could not set penalties");
                    }
                });

                await protocol.GetSessionCompletionTask(cancellationToken);
            }
            catch (Exception e)
            {
                await protocol.SendMessage(new CompetitionError
                {
                    ResponseGuid = firstMessage.ResponseGuid,
                    ErrorMessage = $"Could not login: {e.Message}"
                }, cancellationToken);

                _logger.LogError(e, $"Could not login");
            }
            finally
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetService<EngineControllerContext>();

                var vmRegistration = await context.RegisteredVirtualMachines.FirstOrDefaultAsync(rvm => rvm.VmId == teamVmRegistration.VmId, cancellationToken);
                vmRegistration.IsConnectedNow = false;
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}