namespace ClientCommon.ClientService
{
    public interface IScriptProvider
    {
        #region User utility functions
        public bool UserExists(string username);

        public string[] UserGroups(string username);

        public bool HasEmptyPassword(string username);

        public bool DoesPasswordExpire(string username);

        public bool IsAdmin(string username);

        public bool IsAccountLocked(string username);
        #endregion

        #region System utility functions
        /**
         * Returns an integer status representing the status of the requested service
         * 
         * 0 = not installed/not found
         * 1 = installed, not running or enabled
         * 2 = installed, running but not enabled
         * 3 = installed, not running but enabled
         * 4 = installed, running, and enabled
         */
        public int ServiceStatus(string serviceName);

        /**
         * On Linux, this checks for the package; on Windows, it should check the registry
         */
        public bool ProgramInstalled(string programName);

        /**
         * Program path depends on system; for Linux, it's the package name, for Windows, it's the path to the exe
         * 
         * Returns empty string if program is not found
         */
        public string ProgramVersion(string programPath);

        public bool IsFirewallEnabled();

        public int GlobalMinimumPasswordLength();

        public int GlobalMaxPasswordAge();

        public int GlobalMinPasswordAge();

        public int GlobalPasswordHistoryLength();
        #endregion
    }
}
