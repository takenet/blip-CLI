using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Services.Interfaces
{
    public class BlipAIClientFactory : IBlipAIClientFactory
    {
        public IBlipAIClient GetInstance(string authorizationKey)
        {
            return new BlipHttpClientAsync(authorizationKey);
        }
    }
}
