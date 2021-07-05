using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace EngineController.Models
{
    public class CompetitionSystem
    {
        public int ID { get; set; }

        [DisplayName("README Text")]
        public string ReadmeText { get; set; }

        [DisplayName("System Name")]
        public string SystemIdentifier { get; set; }

        public ICollection<CompetitionPenalty> CompetitionPenalties { get; set; }

        public ICollection<CompetitionTask> CompetitionTasks { get; set; }
    }
}
