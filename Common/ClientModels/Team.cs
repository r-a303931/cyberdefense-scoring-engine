using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCommon.Models
{
	public class Team
	{
		public int ID { get; set; }

		public string Name { get; set; }

		public IEnumerable<AppliedCompetitionPenalty> AppliedCompetitionPenalties { get; set; }
		public IEnumerable<CompletedCompetitionTask> CompletedCompetitionTasks { get; set; }

		public int Points
		{
			get
			{
				var penaltyPoints = (from penalty in AppliedCompetitionPenalties select penalty.CompetitionPenalty.Points).Sum();
				var taskPoints = (from task in CompletedCompetitionTasks select task.CompetitionTask.Points).Sum();

				return taskPoints - penaltyPoints;
			}
		}
	}
}
