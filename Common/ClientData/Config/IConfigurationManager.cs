#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClientCommon.Data.Config
{
    public interface IConfigurationManager
    {
        Task<ServiceConfiguration> LoadConfiguration(CancellationToken cancellationToken = default);

        Task SaveConfiguration(ServiceConfiguration configuration, CancellationToken cancellationToken = default);
    }

    public class ServiceConfiguration
    {
        public int? TeamID { get; set; }
        public int? SystemIdentifier { get; set; }
        public string? EngineControllerHost { get; set; }
        public Guid SystemGUID { get; set; } = default;
    }
}
