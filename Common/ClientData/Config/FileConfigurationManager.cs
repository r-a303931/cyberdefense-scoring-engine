#nullable enable

using System;
using System.Text.Json;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClientCommon.Data.Config
{
    public abstract class FileConfigurationManager : IConfigurationManager
    {
        protected FileConfigurationManager() { }

        public abstract string GetConfigurationFilePath();

        public async Task<ServiceConfiguration> LoadConfiguration(CancellationToken cancellationToken = default)
        {
            using var fileStream = File.OpenRead(GetConfigurationFilePath());
            var loadedServiceConfiguration = await JsonSerializer.DeserializeAsync<ServiceConfiguration>(fileStream, cancellationToken: cancellationToken);

            if (loadedServiceConfiguration is ServiceConfiguration config)
            {
                return config;
            }
            else
            {
                Console.Error.WriteLine("Could not load service configuration; providing empty configuration instead");
                return new ServiceConfiguration { };
            }
        }

        public async Task SaveConfiguration(ServiceConfiguration serviceConfiguration, CancellationToken cancellationToken = default)
        {
            using var fileStream = File.Create(GetConfigurationFilePath());
            await JsonSerializer.SerializeAsync(fileStream, serviceConfiguration, new JsonSerializerOptions
            {
                WriteIndented = true
            }, cancellationToken);
        }
    }
}
