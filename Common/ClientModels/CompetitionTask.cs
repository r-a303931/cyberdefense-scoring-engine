using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCommon.Models
{
	public class CompetitionTask
	{
		public int ID { get; set; }

		public string TaskName { get; set; }

		public string SystemIdentifier { get; set; }

		public int Points { get; set; }


		public ScriptType ScriptType { get; set; }
		public string ValidationScript { get; set; }
	}
}
