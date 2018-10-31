using ITGlobal.CommandLine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Handlers
{
    public class BlipConfigurationHandler : HandlerAsync
    {
        private readonly IBlipClientFactory _blipClientFactory;
        public INamedParameter<string> Authorization { get; set; }
        public INamedParameter<string> Node { get; set; }
        public INamedParameter<string> Payload { get; set; }
        public ISwitch Download { get; set; }

        public BlipConfigurationHandler(IBlipClientFactory blipClientFactory, IInternalLogger logger) : base(logger)
        {
            _blipClientFactory = blipClientFactory;
        }
        public override async Task<int> RunAsync(string[] args)
        {
            var blipConfiguration = _blipClientFactory.GetInstanceForConfiguration(Authorization.Value);
            await blipConfiguration.GetMessengerQRCodeAsync(Node.Value, Verbose.IsSet, Payload.Value, Download.IsSet);
            return 0;
        }
    }
}
