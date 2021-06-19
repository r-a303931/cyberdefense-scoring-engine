using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using ClientCommon.Installer.Utilities;
using Clients.Windows.Constants;

namespace Clients.Windows.Installer
{
    /// <summary>
    /// This program installs and configures the cyber defense scoring engine
    /// </summary>
    public class Program
    {
        static async void Main()
        {
            try
            {
                await Configuration.InstallFiles(Assembly.GetExecutingAssembly(), Constants.Constants.InstallPath, fileMappings: new Hashtable
                {
                    { "Clients.Windows.Installer.Resources.README.url", @"C:\Users\Public\Desktop\README.url" },
                    { "Clients.Windows.Installer.Resources.ScoringReport.url", @"C:\Users\Public\Desktop\Scoring Report.url" }
                }, cancellationToken: System.Threading.CancellationToken.None);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("There was an error!");
            }
            Console.WriteLine("Exiting");
        }
    }
}
