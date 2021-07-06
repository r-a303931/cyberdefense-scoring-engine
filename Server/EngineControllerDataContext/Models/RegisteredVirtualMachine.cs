using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EngineController.Models
{
    public class RegisteredVirtualMachine
    {
        [Key]
        public Guid VmId { get; set; }

        [JsonIgnore]
        public int TeamID { get; set; }
        public Team Team { get; set; }

        [JsonIgnore]
        public int SystemIdentifier { get; set; }
        public CompetitionSystem CompetitionSystem { get; set; }

        public DateTime LastCheckIn { get; set; }

        public bool IsConnectedNow { get; set; }

        [JsonIgnore]
        public ICollection<AppliedCompetitionPenalty> CompetitionPenalties { get; set; }
        [JsonIgnore]
        public ICollection<CompletedCompetitionTask> CompetitionTasks { get; set; }

        [JsonIgnore]
        public int Points
        {
            get
            {
                var penaltyPoints = (
                    from penalty
                    in CompetitionPenalties
                    select penalty.CompetitionPenalty.Points
                ).Sum();
                var taskPoints = (
                    from task
                    in CompetitionTasks
                    select task.CompetitionTask.Points
                ).Sum();

                return taskPoints - penaltyPoints;
            }
        }
    }
}
