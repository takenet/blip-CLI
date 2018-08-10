using ITGlobal.CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Handlers
{
    public class NLPAnalyseHandler : HandlerAsync
    {
        private readonly IBlipClientFactory _blipClientFactory;
        private readonly INLPAnalyseFileReader _fileReader;

        public INamedParameter<string> Input { get; set; }
        public INamedParameter<string> Authorization { get; set; }

        public NLPAnalyseHandler(IBlipClientFactory blipClientFactory, INLPAnalyseFileReader fileReader)
        {
            _blipClientFactory = blipClientFactory;
            _fileReader = fileReader;
        }

        public override async Task<int> RunAsync(string[] args)
        {
            if (!Authorization.IsSet)
                throw new ArgumentNullException("You must provide the target bot (node) for this action. Use '-a' [--authorization] parameters");

            if (!Input.IsSet)
                throw new ArgumentNullException("You must provide the input source (phrase or file) for this action. Use '-i' [--input] parameters");

            if (string.IsNullOrEmpty(Input.Value))
            {
                throw new ArgumentNullException("You must provide the input source (phrase or file) for this action. Your input was empty.");
            }

            string authorization = Authorization.Value;

            var client = _blipClientFactory.GetInstanceForAI(authorization);

            var inputData = Input.Value;

            bool isPhrase = false;

            var isDirectory = _fileReader.IsDirectory(inputData);
            var isFile = _fileReader.IsFile(inputData);

            if (isDirectory)
            {
                LogVerboseLine("\tÉ diretório");
                throw new ArgumentNullException("You must provide the input source (phrase or file) for this action. Your input was a direcory.");
            }
            else
            if (isFile)
            {
                LogVerboseLine("\tÉ arquivo");
                isPhrase = false;
            }
            else
            {
                LogVerboseLine("\tÉ frase");
                isPhrase = true;
            }

            var responses = new List<AnalysisResponse>();

            string EntitiesToString(List<EntityResponse> entities)
            {
                if (entities == null || entities.Count < 1)
                    return string.Empty;
                var toString = string.Join(", ", entities.Select(e => $"{e.Id}:{e.Value}"));
                return toString;
            }

            async Task<AnalysisResponse> AnalyseForMetrics(string input)
            {
                return await client.AnalyseForMetrics(input);
            }

            void ShowResult(AnalysisResponse item)
            {
                responses.Add(item);
                LogVerboseLine($"\"{item.Text}\"\t{item.Intentions[0].Id}:{item.Intentions[0].Score}\t{EntitiesToString(item.Entities.ToList())}");
            }

            var analyseBlock = new TransformBlock<string, AnalysisResponse>((Func<string, Task<AnalysisResponse>>)AnalyseForMetrics);
            var actionBlock = new ActionBlock<AnalysisResponse>((Action<AnalysisResponse>)ShowResult);

            analyseBlock.LinkTo(actionBlock);

            if (isPhrase)
            {
                await analyseBlock.SendAsync(inputData);
            }
            else
            {
                var inputList = await _fileReader.GetInputsToAnalyseAsync(inputData);
                foreach (var input in inputList)
                {
                    await analyseBlock.SendAsync(input);
                }
            }
            analyseBlock.Complete();
            await actionBlock.Completion;

            LogVerboseLine("*********** TESTING ***********");
            foreach (var item in responses)
            {
                LogVerboseLine($"\"{item.Text}\"\t{item.Intentions[0].Id}:{item.Intentions[0].Score}\t{EntitiesToString(item.Entities.ToList())}");
            }

            return 0;
        }
    }
}
