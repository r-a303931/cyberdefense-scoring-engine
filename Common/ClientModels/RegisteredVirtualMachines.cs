using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientCommon.Models
{
    public class RegisteredVirtualMachines
    {
        public Guid Id { get; set; }

        public int TeamId { get; set; }

        public int SystemIdentifier { get; set; }

        public DateTime LastCheckIn { get; set; }
    }
}
