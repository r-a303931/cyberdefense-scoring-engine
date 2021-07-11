using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ClientCommon.Data.Config
{
    public class TestConfigurationManager : IConfigurationManager
    {
        public Task<ServiceConfiguration> LoadConfiguration(CancellationToken cancellationToken) =>
            Task.FromResult(new ServiceConfiguration
            {
                TeamID = null,
                SystemIdentifier = 1,
                EngineControllerHost = "127.0.0.1",
                SystemGUID = Guid.Empty
            });
        public Task<ServiceConfiguration> LoadConfiguration() => LoadConfiguration(default);

        public Task SaveConfiguration(ServiceConfiguration serviceConfiguration) => SaveConfiguration(serviceConfiguration, default);

        public Task SaveConfiguration(ServiceConfiguration serviceConfiguration, CancellationToken cancellationToken)
        {
            Console.WriteLine("Saving configuration:");
            Console.WriteLine(JsonSerializer.Serialize(serviceConfiguration, new JsonSerializerOptions
            {
                WriteIndented = true
            }));

            return Task.CompletedTask;
        }
    }
}
