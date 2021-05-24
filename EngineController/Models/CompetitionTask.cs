using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineController.Models
{
	public class CompetitionTask
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int ID { get; set; }

		[DisplayName("Name")]
		public string TaskName { get; set; }

		[DisplayName("System Identifier")]
		public string SystemIdentifier { get; set; }

		public int Points { get; set; }


		[DisplayName("Script Type")]
		public ScriptType ScriptType { get; set; }
		[DisplayName("Task Verification Script")]
		public string ValidationScript { get; set; }
	}
}
