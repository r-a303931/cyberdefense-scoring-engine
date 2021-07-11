using System;

using ClientCommon.ClientService;

namespace ClientCommon.WebInterface.Stubs
{
    public class ScriptProviderStub : IScriptProvider
    {
        public bool DoesPasswordExpire(string username)
        {
            return false;
        }

        public int GlobalMaxPasswordAge()
        {
            return 0;
        }

        public int GlobalMinimumPasswordLength()
        {
            return 0;
        }

        public int GlobalMinPasswordAge()
        {
            return 0;
        }

        public int GlobalPasswordHistoryLength()
        {
            return 0;
        }

        public bool HasEmptyPassword(string username)
        {
            return true;
        }

        public bool IsAccountLocked(string username)
        {
            return false;
        }

        public bool IsAdmin(string username)
        {
            return true;
        }

        public bool IsFirewallEnabled()
        {
            return false;
        }

        public bool ProgramInstalled(string programName)
        {
            return false;
        }

        public string ProgramVersion(string programPath)
        {
            return "";
        }

        public int ServiceStatus(string serviceName)
        {
            return 4;
        }

        public bool UserExists(string username)
        {
            return false;
        }

        public string[] UserGroups(string username)
        {
            return Array.Empty<string>();
        }
    }
}
