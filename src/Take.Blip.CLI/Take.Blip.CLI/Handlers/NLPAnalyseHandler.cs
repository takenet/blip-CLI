using ITGlobal.CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;

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

            if (isDirectory && !isFile)
            {
                LogVerbose("É diretório");
                throw new ArgumentNullException("You must provide the input source (phrase or file) for this action. Your input was a direcory.");
            }
            else
            if (isDirectory && isFile)
            {
                LogVerbose("É arquivo");
                isPhrase = false;
            }
            else
            {
                isPhrase = true;
            }


            if (isPhrase)
            {
                await client.AnalyseForMetrics(inputData);
            }
            else
            {
                var inputList = await _fileReader.GetInputsToAnalyseAsync(inputData);
                foreach (var input in inputList)
                {
                    await client.AnalyseForMetrics(input);
                }
            }

            return 0;
        }
    }
}
