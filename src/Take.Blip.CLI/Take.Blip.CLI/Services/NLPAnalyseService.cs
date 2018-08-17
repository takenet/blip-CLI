using ITGlobal.CommandLine;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Take.BlipCLI.Models;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Services
{
    public class NLPAnalyseService : INLPAnalyseService
    {
        private readonly IBlipClientFactory _blipClientFactory;
        private readonly IFileManagerService _fileService;
        private readonly ILogger _logger;

        public NLPAnalyseService(IBlipClientFactory blipClientFactory, IFileManagerService fileService, ILogger logger)
        {
            _blipClientFactory = blipClientFactory;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task AnalyseAsync(string authorization, string inputSource, string reportOutput)
        {
            if(string.IsNullOrEmpty(authorization))
                throw new ArgumentNullException("You must provide the target bot (node) for this action.");

            if (string.IsNullOrEmpty(inputSource))
                throw new ArgumentNullException("You must provide the input source (phrase or file) for this action.");

            if (string.IsNullOrEmpty(reportOutput))
                throw new ArgumentNullException("You must provide the full output's report file name for this action.");
            
            _fileService.CreateDirectoryIfNotExists(reportOutput);
           

            var client = _blipClientFactory.GetInstanceForAI(authorization);
            

            bool isPhrase = false;

            var isDirectory = _fileService.IsDirectory(inputSource);
            var isFile = _fileService.IsFile(inputSource);

            if (isFile)
            {
                _logger.LogDebug("\tÉ arquivo\n");
                isPhrase = false;
            }
            else
            if (isDirectory)
            {
                _logger.LogDebug("\tÉ diretório\n");
                throw new ArgumentNullException("You must provide the input source (phrase or file) for this action. Your input was a direcory.");
            }
            else
            {
                _logger.LogDebug("\tÉ frase\n");
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

            async Task<AnalysisResponse> AnalyseForMetrics(string request)
            {
                return await client.AnalyseForMetrics(request);
            }

            void ShowResult(AnalysisResponse item)
            {
                if (item == null)
                    return;
                responses.Add(item);
                _logger.LogDebug($"\"{item.Text}\"\t{item.Intentions?[0].Id}:{item.Intentions?[0].Score:P}\t{EntitiesToString(item.Entities?.ToList())}\n");
            }

            var options = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = DataflowBlockOptions.Unbounded,
                MaxDegreeOfParallelism = 10
            };

            var analyseBlock = new TransformBlock<string, AnalysisResponse>((Func<string, Task<AnalysisResponse>>)AnalyseForMetrics, options);
            var actionBlock = new ActionBlock<AnalysisResponse>((Action<AnalysisResponse>)ShowResult, options);

            analyseBlock.LinkTo(actionBlock, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });

            if (isPhrase)
            {
                await analyseBlock.SendAsync(inputSource);
            }
            else
            {
                var inputList = await _fileService.GetInputsToAnalyseAsync(inputSource);
                foreach (var input in inputList)
                {
                    await analyseBlock.SendAsync(input);
                }
            }
            analyseBlock.Complete();
            await actionBlock.Completion;

            var report = new NLPAnalyseReport
            {
                AnalysisResponses = responses,
                FullReportFileName = reportOutput
            };

            await _fileService.WriteAnalyseReportAsync(report);
            
        }
    }
}
