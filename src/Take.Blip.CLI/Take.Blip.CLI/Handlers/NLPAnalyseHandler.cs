using ITGlobal.CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Take.BlipCLI.Models;
using Take.BlipCLI.Models.NLPAnalyse;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Handlers
{
    public class NLPAnalyseHandler : HandlerAsync
    {
        private readonly INLPAnalyseService _analyseService;

        public INamedParameter<string> Input { get; set; }
        public INamedParameter<string> Authorization { get; set; }
        public INamedParameter<string> ReportOutput { get; set; }
        public ISwitch DoContentCheck { get; set; }
        public ISwitch Raw { get; set; }
        public INamedParameter<int> ApplyJitter { get; set; }

        public NLPAnalyseHandler(INLPAnalyseService analyseService, IInternalLogger logger) : base(logger)
        {
            _analyseService = analyseService;
        }

        public override async Task<int> RunAsync(string[] args)
        {
            if (!Authorization.IsSet)
                throw new ArgumentNullException("You must provide the target bot (node) for this action. Use '-a' [--authorization] parameters");

            if (!Input.IsSet)
                throw new ArgumentNullException("You must provide the input source (phrase or file) for this action. Use '-i' [--input] parameters");

            if (!ReportOutput.IsSet)
                throw new ArgumentNullException("You must provide the full output's report file name for this action. Use '-o' [--output] parameters");
            
            var parameters = new AnalyseParameters
            {
                Authorization = Authorization.Value,
                InputSource = Input.Value,
                ReportOutput = ReportOutput.Value,
                DoContentCheck = DoContentCheck.IsSet,
                ShowRawContent = Raw.IsSet,
                JitterSize = ApplyJitter.IsSet ? ApplyJitter.Value : 0
            };

            await _analyseService.AnalyseAsync(parameters);

            return 0;
        }
    }
}
