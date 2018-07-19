using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface IBlipAIClientFactory
    {
        IBlipAIClient GetInstance(string authorizationKey);
    }
}
