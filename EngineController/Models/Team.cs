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
				var penaltyPoints = (from penalty in AppliedCompetitionPenalties select penalty.CompetitionPenalty.Points).Sum();
				var taskPoints = (from task in CompletedCompetitionTasks select task.CompetitionTask.Points).Sum();

				return taskPoints - penaltyPoints;
			}
		}

		public ICollection<RegisteredVirtualMachines> RegisteredVirtualMachines { get; set; }
	}
}
