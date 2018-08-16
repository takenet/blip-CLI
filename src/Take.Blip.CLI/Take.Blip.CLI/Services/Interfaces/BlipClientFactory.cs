using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Services.Interfaces
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
