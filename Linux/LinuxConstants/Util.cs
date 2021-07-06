using System;
using System.Runtime.InteropServices;

namespace Clients.Linux.Constants
{
    public class Util
    {
        [DllImport("libc")]
        private static extern uint getuid();

        public static void EnsureUserIsRoot()
        {
            if (getuid() != 0)
            {
                Console.Error.WriteLine("Rerun the command as root");
                throw new Exception();
            }
        }
    }
}
