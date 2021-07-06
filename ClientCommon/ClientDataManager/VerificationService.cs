using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ClientCommon.Data.Config;
using ClientCommon.Data.InformationContext;

namespace ClientCommon.ClientService
{
    public class VerificationService : BackgroundService
    {
        private readonly ILogger<VerificationService> _logger;
        private readonly IScriptProvider _scriptProvider;
        private readonly IClientInformationContext _informationContext;
        private readonly IConfigurationManager _configurationManager;

        public VerificationService(ILogger<VerificationService> logger, IScriptProvider scriptProvider, IClientInformationContext informationContext, IConfigurationManager configuration)
        {
            _logger = logger;
            _scriptProvider = scriptProvider;
            _informationContext = informationContext;
            _configurationManager = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(500, cancellationToken);
            }
        }
    }
}
