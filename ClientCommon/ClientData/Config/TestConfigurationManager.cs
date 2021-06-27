using System.Threading;
using System.Threading.Tasks;

namespace ClientCommon.Data.Config
{
    public class TestConfigurationManager : IConfigurationManager
    {
        public Task<ServiceConfiguration> LoadConfiguration(CancellationToken cancellationToken) =>
            Task.FromResult(new ServiceConfiguration
            {
                TeamID = 1,
                SystemIdentifier = 1
            });
        public Task<ServiceConfiguration> LoadConfiguration() => LoadConfiguration(new CancellationToken());

        public Task SaveConfiguration(ServiceConfiguration serviceConfiguration) => Task.CompletedTask;
        public Task SaveConfiguration(ServiceConfiguration serviceConfiguration, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
