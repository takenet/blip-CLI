using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface IBlipConfigurationClient
    {
        Task<string> GetMessengerQRCodeAsync(string node, bool verbose = false, string payload = "", bool download = false);
        Task<bool> PingAsync(string node);
    }
}
