using ITGlobal.CommandLine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Take.BlipCLI.Handlers
{
    public class PingHandler : HandlerAsync
    {
        public INamedParameter<string> Node { get; set; }
        
        public override async Task<int> RunAsync(string[] args)
        {
            return 0;
        }
    }
}
