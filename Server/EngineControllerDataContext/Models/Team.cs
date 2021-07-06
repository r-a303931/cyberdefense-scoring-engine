using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineController.Models
{
    public class Team
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [EditableAttribute(true)]
        public string Name { get; set; }

        public ICollection<AppliedCompetitionPenalty> AppliedCompetitionPenalties { get; set; }
        public ICollection<CompletedCompetitionTask> CompletedCompetitionTasks { get; set; }

        public int Points
        {
            get
            {
                var penaltyPoints = (
                    from penalty
                    in AppliedCompetitionPenalties
                    group penalty by penalty.VmId into penaltyGroup
                    select penaltyGroup.First().CompetitionPenalty.Points
                ).Sum();
                var taskPoints = (
                    from task
                    in CompletedCompetitionTasks
                    group task by task.VmId into taskGroup
                    select taskGroup.First().CompetitionTask.Points
                ).Sum();

                return taskPoints - penaltyPoints;
            }
        }

        public ICollection<RegisteredVirtualMachine> RegisteredVirtualMachines { get; set; }
    }
}
