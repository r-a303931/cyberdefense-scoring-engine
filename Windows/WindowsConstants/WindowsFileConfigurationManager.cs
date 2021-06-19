using ClientCommon.Data.Config;

namespace Clients.Windows.Constants
{
    public class WindowsFileConfigurationManager : FileConfigurationManager
    {
        public override string GetConfigurationFilePath() => Constants.ConfigurationPath;
    }
}
