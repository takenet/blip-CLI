using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Services
{
    public class BlipClientFactory : IBlipClientFactory
    {
        private readonly IInternalLogger _logger;

        public BlipClientFactory(IInternalLogger logger)
        {
            _logger = logger;
        }

        public IBlipAIClient GetInstanceForAI(string authorizationKey)
        {
            return new BlipHttpClientAsync(authorizationKey, _logger);
        }

        public IBlipBucketClient GetInstanceForBucket(string authorizationKey)
        {
            return new BlipHttpClientAsync(authorizationKey, _logger);
        }

        public IBlipConfigurationClient GetInstanceForConfiguration(string authorizationKey)
        {
            return new BlipHttpClientAsync(authorizationKey, _logger);
        }
    }
}
