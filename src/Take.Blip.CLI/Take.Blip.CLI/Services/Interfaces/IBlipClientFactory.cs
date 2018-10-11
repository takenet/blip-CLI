using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface IBlipClientFactory
    {
        IBlipAIClient GetInstanceForAI(string authorizationKey);
        IBlipBucketClient GetInstanceForBucket(string authorizationKey);
        IBlipConfigurationClient GetInstanceForConfiguration(string authorizationKey);
    }
}
