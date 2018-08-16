using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Services
{
    public class BlipClientFactory : IBlipClientFactory
    {
        public IBlipAIClient GetInstanceForAI(string authorizationKey)
        {
            return new BlipHttpClientAsync(authorizationKey);
        }

        public IBlipBucketClient GetInstanceForBucket(string authorizationKey)
        {
            return new BlipHttpClientAsync(authorizationKey);
        }
    }
}
