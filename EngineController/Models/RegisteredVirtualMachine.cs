using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EngineController.Models
{
    public class RegisteredVirtualMachine
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [JsonIgnore]
        public int TeamID { get; set; }
        public Team Team { get; set; }

        [JsonIgnore]
        public int SystemIdentifier { get; set; }
        public CompetitionSystem CompetitionSystem { get; set; }

        public DateTime LastCheckIn { get; set; }
    }
}
