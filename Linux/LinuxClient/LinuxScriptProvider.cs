using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Common;
using ClientCommon.ClientService;

namespace Clients.Linux.Main
{
    internal class LinuxScriptProvider : IScriptProvider
    {
        #region User utility functions
        public bool UserExists(string username) =>
            File.ReadAllLines("/etc/passwd")
                .Any(line => line.Split(":")[0] == username);

        public string[] UserGroups(string username) =>
            File.ReadAllLines("/etc/group")
                .First(line => line.Split(":")[0] == username)?
                .Split(":")?[3]?
                .Split(",");

        public bool HasEmptyPassword(string username)
        {
            var password = File.ReadAllLines("/etc/shadow")
                .First(line => line.Split(":")[0] == username)?
                .Split(":")?[1];

            return password == null || password == "*";
        }

        public bool DoesPasswordExpire(string username) =>
            File.ReadAllLines("/etc/shadow")
                .First(line => line.Split(":")[0] == username)?
                .Split(":")?[4] == "99999";

        public bool IsAdmin(string username) =>
            UserGroups(username)
                .Any(group => group == "sudo" || group == "adm");

        public bool IsAccountLocked(string username) =>
            File.ReadAllLines("/etc/shadow")
                .First(line => line.Split(":")[0] == username)?
                .Split(":")?[1]?
                .StartsWith("!")
                ?? false;
        #endregion

        #region System utility functions
        public int ServiceStatus(string serviceName)
        {
            var isEnabledResults = ProcessManagement.RunProcessForOutput("systemctl", $"is-enabled {serviceName}").Result;
            var isActiveResults = ProcessManagement.RunProcessForOutput("systemctl", $"is-active {serviceName}").Result;

            if (isEnabledResults.StandardErrorLines.Count() == 1)
            {
                return 0;
            }
            else if (isEnabledResults.ExitCode == 0)
            {
                return isActiveResults.ExitCode == 0
                    ? 4
                    : 3;
            }
            else
            {
                return isActiveResults.ExitCode == 0
                    ? 2
                    : 1;
            }
        }

        public bool ProgramInstalled(string programName) =>
            ProcessManagement.RunProcessForOutput("dpkg", "-l")
                .Result
                .StandardOutputLines
                .Select(line => line.Split(" ", StringSplitOptions.RemoveEmptyEntries)[1])
                .Any(program => program == programName);

        public string ProgramVersion(string programName) =>
            ProcessManagement.RunProcessForOutput("dpkg", "-l")
                .Result
                .StandardOutputLines
                .Select(line => line.Split(" ", StringSplitOptions.RemoveEmptyEntries))
                .First(line => line[1] == programName)?[2] ?? "";

        public bool IsFirewallEnabled() =>
            ProcessManagement.RunProcessForOutput("ufw", "status")
                .Result
                .StandardOutputLines
                .ToList()[0]?
                .Trim() == "Status: active";

        public int GlobalMinimumPasswordLength()
        {
            var matches = Regex.Matches(File.ReadAllText("/etc/pam.d/common-password"), @"minlen=(\d*)");

            if (matches.Count == 0)
            {
                return 0;
            }
            else
            {
                return int.Parse(matches[0].Groups[1].Value);
            }
        }

        public int GlobalMinPasswordAge() =>
            int.Parse(
                File.ReadAllLines("/etc/login.defs")
                    .Select(line => line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries))
                    .First(line => line[0] == "PASS_MIN_DAYS")?[1] ?? "0"
            );

        public int GlobalMaxPasswordAge() =>
            int.Parse(
                File.ReadAllLines("/etc/login.defs")
                    .Select(line => line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries))
                    .First(line => line[0] == "PASS_MAX_DAYS")?[1] ?? "0"
            );

        public int GlobalPasswordHistoryLength()
        {
            var matches = Regex.Matches(File.ReadAllText("/etc/pam.d/common-password"), @"remember=(\d*)");

            if (matches.Count == 0)
            {
                return 0;
            }
            else
            {
                return int.Parse(matches[0].Groups[1].Value);
            }
        }
        #endregion
    }
}

