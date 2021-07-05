using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Common.Models
{
    public class CompetitionSystem
    {
        public int ID { get; set; }

        [DisplayName("README Text")]
        public string ReadmeText { get; set; }

        [DisplayName("System Name")]
        public string SystemIdentifier { get; set; }

        public IEnumerable<CompetitionPenalty> CompetitionPenalties { get; set; }

        public IEnumerable<CompetitionTask> CompetitionTasks { get; set; }
    }
}
