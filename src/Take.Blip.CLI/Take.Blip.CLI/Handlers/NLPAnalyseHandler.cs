using ITGlobal.CommandLine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Services;

namespace Take.BlipCLI.Handlers
{
    public class NLPAnalyseHandler : HandlerAsync
    {
        public INamedParameter<string> Text { get; set; }
        public INamedParameter<string> Node { get; set; }
        public INamedParameter<string> AccessKey { get; set; }

        public override async Task<int> RunAsync(string[] args)
        {
            var client = new BlipHttpClientAsync(AccessKey.Value);
            await client.AnalyseForMetrics(Text.Value);
            return 0;
        }
    }   
}
