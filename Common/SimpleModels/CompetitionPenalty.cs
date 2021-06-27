using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
	public class CompetitionPenalty
	{
        public int ID { get; set; }

		public string PenaltyName { get; set; }

		public int SystemIdentifier { get; set; }

		public int Points { get; set; }


		public ScriptType ScriptType { get; set; }
		public string PenaltyScript { get; set; }
	}
}
