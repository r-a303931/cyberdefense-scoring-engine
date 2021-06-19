using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientService
{
    interface IScriptProvider
    {
#region User utility functions
        bool UserExists(string username);

        string[] UserGroups(string username);

        bool HasEmptyPassword(string username);

        bool DoesPasswordExpire(string username);

        bool IsAdmin(string username);
#endregion

#region System utility functions
        /**
         * Returns an integer status representing the status of the requested service
         * 
         * 0 = not installed/not found
         * 1 = installed, not running or enabled
         * 2 = installed, running but not enabled
         * 3 = installed, running, and enabled
         */
        int ServiceStatus(string serviceName);

        bool ProgramInstalled(string programName);

        /**
         * Program path depends on system; for Linux, it's the package name, for Windows, it's the path to the exe
         */
        string ProgramVersion(string programPath);

        bool CheckFirewallEnabled();

        int GlobalMinimumPasswordLength();

        int GlobalMaxPasswordAge();

        int GlobalMinPasswordAge();

        int GlobalPasswordHistoryLength();
#endregion
    }
}
