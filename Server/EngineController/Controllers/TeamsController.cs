using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EngineController.Data;
using EngineController.Models;
using Microsoft.EntityFrameworkCore;

namespace EngineController.Controllers
{
    [ApiController]
    [Route("api/teams")]
    [Produces("application/json")]
    public class TeamsController : ControllerBase
    {
        private readonly EngineControllerContext _context;

        public TeamsController(EngineControllerContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return new JsonResult(
                await _context.Teams
                    .Include(t => t.AppliedCompetitionPenalties)
                    .ThenInclude(p => p.CompetitionPenalty)
                    .Include(t => t.CompletedCompetitionTasks)
                    .ThenInclude(p => p.CompetitionTask)
                    .ToListAsync()
            );
        }

        [HttpPost]
        [Route("completetask")]
        public async Task<IActionResult> CompleteTask(int teamID, [FromBody] int[] taskIDs)
        {
            if (!await TeamExists(teamID) || !await TasksExist(taskIDs))
            {
                return NotFound();
            }

            var team = await _context.Teams
                .Include(t => t.CompletedCompetitionTasks)
                .FirstOrDefaultAsync(team => team.ID == teamID);

            IEnumerable<int> collectionTaskIDs = from task
                                                 in team.CompletedCompetitionTasks
                                                 select task.CompetitionTaskID;

            IEnumerable<int> taskIDsToBeAdded = taskIDs.Except(collectionTaskIDs);
            IEnumerable<int> taskIDsToBeRemoved = collectionTaskIDs.Except(taskIDs);

            foreach (var taskIDToAdd in taskIDsToBeAdded)
            {
                team.CompletedCompetitionTasks.Add(new CompletedCompetitionTask
                {
                    TeamID = team.ID,
                    CompetitionTaskID = taskIDToAdd
                });
            }

            foreach (var taskIDToRemove in taskIDsToBeRemoved)
            {
                team.CompletedCompetitionTasks.Remove(new CompletedCompetitionTask
                {
                    TeamID = team.ID,
                    CompetitionTaskID = taskIDToRemove
                });
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        [Route("applypenalty")]
        public async Task<IActionResult> ApplyPenalty(int teamID, [FromBody] int[] penaltyIDs)
        {
            if (!await TeamExists(teamID) || !await PenaltiesExist(penaltyIDs))
            {
                return NotFound();
            }

            var team = await _context.Teams
                .Include(t => t.AppliedCompetitionPenalties)
                .FirstOrDefaultAsync(team => team.ID == teamID);

            IEnumerable<int> competitionPenaltyIDs = from task
                                                 in team.AppliedCompetitionPenalties
                                                     select task.CompetitionPenaltyID;

            IEnumerable<int> taskIDsToBeAdded = penaltyIDs.Except(competitionPenaltyIDs);
            IEnumerable<int> taskIDsToBeRemoved = competitionPenaltyIDs.Except(penaltyIDs);

            foreach (var penaltyIDToAdd in taskIDsToBeAdded)
            {
                team.AppliedCompetitionPenalties.Add(new AppliedCompetitionPenalty
                {
                    TeamID = team.ID,
                    CompetitionPenaltyID = penaltyIDToAdd
                });
            }

            foreach (var penaltyIDToRemove in taskIDsToBeRemoved)
            {
                team.AppliedCompetitionPenalties.Remove(new AppliedCompetitionPenalty
                {
                    TeamID = team.ID,
                    CompetitionPenaltyID = penaltyIDToRemove
                });
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        private Task<bool> TeamExists(int teamID)
        {
            return _context.Teams.AnyAsync(team => team.ID == teamID);
        }

        private async Task<bool> TasksExist(int[] taskIDs)
        {
            foreach (int taskID in taskIDs)
            {
                if (!await _context.CompetitionTask.AnyAsync(task => task.ID == taskID))
                {
                    return false;
                }
            }
            return true;
        }

        private async Task<bool> PenaltiesExist(int[] penaltyIDs)
        {
            foreach (int penaltyID in penaltyIDs)
            {
                if (!await _context.CompetitionPenalty.AnyAsync(penalty => penalty.ID == penaltyID))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
