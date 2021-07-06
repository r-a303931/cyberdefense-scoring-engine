using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Common;
using ClientCommon.ClientService;
using Microsoft.Win32;

namespace Clients.Windows.Main
{
    internal class WindowsScriptProvider : IScriptProvider
    {
        #region User utility functions
        public bool UserExists(string username) =>
            ProcessManagement.RunProcessForOutput("net.exe", $"user {username}")
                .Result
                .ExitCode == 0;

        public string[] UserGroups(string username)
        {
            var result = ProcessManagement.RunProcessForOutput("net.exe", $"user {username}").Result;

            List<string> groups = new();

            bool recordingGroups = false;

            foreach (var line in result.StandardOutputLines)
            {
                if (!recordingGroups && line.StartsWith("Local Group Memberships"))
                {
                    recordingGroups = true;

                    var parts = line.Split(new char[] { '*', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    groups.AddRange(parts.AsEnumerable().Skip(1));
                }
                else if (recordingGroups && line.StartsWith("Global Group memberships"))
                {
                    return groups.ToArray();
                }
                else
                {
                    var parts = line.Split(new char[] { '*', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    groups.AddRange(parts.AsEnumerable());
                }
            }

            return Array.Empty<string>();
        }

        public bool HasEmptyPassword(string username)
        {
            var result = new IntPtr();
            var loggedInResult = Win32Api.LogonUserW(
                username,
                null,
                "",
                Win32Api.GetLogon32_Logon_Network(),
                Win32Api.GetLogon32_Provider_Default(),
                ref result
            ) != 0;

            var err = Win32Api.GetLastError();

            _ = Win32Api.CloseHandle(result);

            // Error 1327 means empty password
            // So, it could succeed with an empty password: they have an empty password
            // Or, it could throw an error because of the empty password: they have an empty password
            return !(loggedInResult || err == 1327);
        }

        public bool DoesPasswordExpire(string username) =>
            !ProcessManagement.RunProcessForOutput("net.exe", $"user {username}")
                .Result
                .OutputLines
                .First(line => line.StartsWith("Password expires"))
                .Contains("Never");

        public bool IsAdmin(string username) =>
            UserGroups(username)
                .Any(group => group == "Administrators");

        public bool IsAccountLocked(string username) =>
            ProcessManagement.RunProcessForOutput("net.exe", $"user {username}")
                .Result
                .OutputLines
                .First(line => line.StartsWith("Account active"))
                .Contains("No");
        #endregion

        #region System utility functions
        public int ServiceStatus(string serviceName)
        {
            var queryResult = ProcessManagement.RunProcessForOutput("sc.exe", $"query {serviceName}").Result;

            if (queryResult.ExitCode == 1060)
            {
                return 0;
            }

            var queryConfigResult = ProcessManagement.RunProcessForOutput("sc.exe", $"qc {serviceName}").Result;

            var enabledAtBoot = queryConfigResult
                .StandardOutputLines
                .First(line => line.Trim().StartsWith("START_TYPE"))
                .EndsWith("AUTO_START") || queryConfigResult
                .StandardOutputLines
                .First(line => line.Trim().StartsWith("START_TYPE"))
                .EndsWith("AUTO_START  (DELAYED)");

            var running = queryResult
                .StandardOutputLines
                .First(line => line.Trim().StartsWith("STATE"))
                .EndsWith("RUNNING");

            if (running)
            {
                return enabledAtBoot
                    ? 4
                    : 2;
            }
            else
            {
                return enabledAtBoot
                    ? 3
                    : 1;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Target requires Windows; Windows -> WindowsClient -> WindowsScriptProvider")]
        public bool ProgramInstalled(string programName)
        {
            var registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

            using var key = Registry.LocalMachine.OpenSubKey(registryKey);
            foreach (var subkeyName in key.GetSubKeyNames())
            {
                using var subkey = key.OpenSubKey(subkeyName);

                if ((string)subkey.GetValue("DisplayName") == programName || subkeyName == programName)
                {
                    return true;
                }
            }

            return false;
        }

        public string ProgramVersion(string programPath)
        {
            var queryResult = ProcessManagement.RunProcessForOutput("wmic.exe", $"datafile where 'name=\"{programPath}\"' get version").Result;
            var lines = queryResult.StandardOutputLines.ToList();

            if (lines.Count == 0 || queryResult.ExitCode != 0)
            {
                return "";
            }

            return lines[1];
        }

        public bool IsFirewallEnabled() =>
            ProcessManagement.RunProcessForOutput("netsh.exe", "advfirewall show allprofiles")
                .Result
                .OutputLines
                .Where(line => line.StartsWith("State"))
                .Select(line => line.Contains("ON"))
                .Aggregate(true, (accum, on) => accum && on);

        public int GlobalMinimumPasswordLength() =>
            int.Parse(
                ProcessManagement.RunProcessForOutput("net", "accounts")
                    .Result
                    .OutputLines
                    .First(line => line.StartsWith("Minimum password length"))
                    .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[1]
            );

        public int GlobalMaxPasswordAge() =>
            int.Parse(
                ProcessManagement.RunProcessForOutput("net", "accounts")
                    .Result
                    .OutputLines
                    .First(line => line.StartsWith("Maximum password age"))
                    .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[1]
            );

        public int GlobalMinPasswordAge() =>
            int.Parse(
                ProcessManagement.RunProcessForOutput("net", "accounts")
                    .Result
                    .OutputLines
                    .First(line => line.StartsWith("Minimum password age"))
                    .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[1]
            );

        public int GlobalPasswordHistoryLength()
        {
            var result = ProcessManagement.RunProcessForOutput("net", "accounts")
                .Result
                .OutputLines
                .First(line => line.StartsWith("Length of password history maintained"))
                .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[1];

            return result == "None"
                ? 0
                : int.Parse(result);
        }
        #endregion
    }

    class Win32Api
    {

        [DllImport("Advapi32.dll", CharSet = CharSet.Unicode)]
        internal static extern int LogonUserW(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            uint dwLogonType,
            uint dwLogonProvider,
            ref IntPtr phandle
        );

        [DllImport("Kernel32.dll")]
        internal static extern uint GetLastError();

        [DllImport("Kernel32.dll")]
        internal static extern int CloseHandle(IntPtr handle);

        [DllImport("Win32Constants.dll")]
        internal static extern uint GetLogon32_Logon_Network();

        [DllImport("Win32Constants.dll")]
        internal static extern uint GetLogon32_Provider_Default();
    }
}
