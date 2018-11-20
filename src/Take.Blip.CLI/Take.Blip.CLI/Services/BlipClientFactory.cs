using Lime.Protocol.Serialization;
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
        private readonly IDocumentTypeResolver _typeResolver;

        public BlipClientFactory(IInternalLogger logger, IDocumentTypeResolver typeResolver)
        {
            _logger = logger;
            _typeResolver = typeResolver;
        }

        public IBlipAIClient GetInstanceForAI(string authorizationKey)
        {
            return new BlipHttpClientAsync(authorizationKey, _logger, _typeResolver);
        }

        public IBlipBucketClient GetInstanceForBucket(string authorizationKey)
        {
            return new BlipHttpClientAsync(authorizationKey, _logger, _typeResolver);
        }

        public IBlipConfigurationClient GetInstanceForConfiguration(string authorizationKey)
        {
            return new BlipHttpClientAsync(authorizationKey, _logger, _typeResolver);
        }
    }
}
