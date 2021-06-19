using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EngineController.Models
{
	public class CompetitionPenalty
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ID { get; set; }

		[DisplayName("Name")]
		public string PenaltyName { get; set; }

		[DisplayName("System Identifier")]
		public int SystemIdentifier { get; set; }

		public int Points { get; set; }


		[DisplayName("Script Type")]
		public ScriptType ScriptType { get; set; }
		[DisplayName("Penalty Verification Script")]
		public string PenaltyScript { get; set; }

		[JsonIgnore]
		public CompetitionSystem CompetitionSystem;
	}
}
