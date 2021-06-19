using ClientCommon.Data.Config;

namespace Clients.Linux.Constants
{
    public class LinuxFileConfigurationManager : FileConfigurationManager
    {
        public override string GetConfigurationFilePath() => Constants.ConfigurationPath;
    }
}
