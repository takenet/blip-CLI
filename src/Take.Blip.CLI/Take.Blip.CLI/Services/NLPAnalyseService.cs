using ITGlobal.CommandLine;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Take.BlipCLI.Models;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;
using Take.ContentProvider.Domain.Contract.Enums;
using Take.ContentProvider.Domain.Contract.Interfaces;
using Take.ContentProvider.Domain.Contract.Model;
using Take.ContentProvider.Infra.Bucket;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Services
{
    public class NLPAnalyseService : INLPAnalyseService
    {
        private readonly IBlipClientFactory _blipClientFactory;
        private readonly IFileManagerService _fileService;
        private readonly IInternalLogger _logger;

        private static object _locker = new object();
        private static int _count = 0;
        private static int _total = 0;

        public NLPAnalyseService(
            IBlipClientFactory blipClientFactory,
            IFileManagerService fileService,
            IInternalLogger logger)
        {
            _blipClientFactory = blipClientFactory;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task AnalyseAsync(string authorization, string inputSource, string reportOutput, bool doContentCheck = false)
        {
            if (string.IsNullOrEmpty(authorization))
                throw new ArgumentNullException("You must provide the target bot (node) for this action.");

            if (string.IsNullOrEmpty(inputSource))
                throw new ArgumentNullException("You must provide the input source (phrase or file) for this action.");

            if (string.IsNullOrEmpty(reportOutput))
                throw new ArgumentNullException("You must provide the full output's report file name for this action.");

            _logger.LogDebug("COMEÇOU!");

            _fileService.CreateDirectoryIfNotExists(reportOutput);

            var bucketStorage = new BucketStorage("Key " + authorization);
            var contentProvider = new Take.ContentProvider.ContentProvider(bucketStorage, 5);
            var client = _blipClientFactory.GetInstanceForAI(authorization);

            _logger.LogDebug("\tCarregando intencoes...");
            var allIntents = await client.GetAllIntentsAsync();
            _logger.LogDebug("\tCarregadas!");

            bool isPhrase = false;

            var isDirectory = _fileService.IsDirectory(inputSource);
            var isFile = _fileService.IsFile(inputSource);

            if (isFile)
            {
                _logger.LogDebug("\tA entrada é um arquivo");
                isPhrase = false;
            }
            else
            if (isDirectory)
            {
                _logger.LogError("\tA entrada é um diretório");
                throw new ArgumentNullException("You must provide the input source (phrase or file) for this action. Your input was a direcory.");
            }
            else
            {
                _logger.LogDebug("\tA entrada é uma frase");
                isPhrase = true;
            }

            var options = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = DataflowBlockOptions.Unbounded,
                MaxDegreeOfParallelism = 20,
                
            };

            var analyseBlock = new TransformBlock<NLPAnalyseDataBlock, NLPAnalyseDataBlock>((Func<NLPAnalyseDataBlock, Task<NLPAnalyseDataBlock>>)AnalyseForMetrics, options);
            var checkBlock = new TransformBlock<NLPAnalyseDataBlock, NLPAnalyseDataBlock>((Func<NLPAnalyseDataBlock, NLPAnalyseDataBlock>)CheckResponse, options);
            var contentBlock = new TransformBlock<NLPAnalyseDataBlock, NLPAnalyseDataBlock>((Func<NLPAnalyseDataBlock, Task<NLPAnalyseDataBlock>>)GetContent, options);
            var showResultBlock = new ActionBlock<NLPAnalyseDataBlock>(BuildResult, new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = DataflowBlockOptions.Unbounded,
                MaxMessagesPerTask = 1
            });

            analyseBlock.LinkTo(checkBlock, new DataflowLinkOptions { PropagateCompletion = true });
            checkBlock.LinkTo(contentBlock, new DataflowLinkOptions { PropagateCompletion = true });
            contentBlock.LinkTo(showResultBlock, new DataflowLinkOptions { PropagateCompletion = true });

            _count = 0;

            var inputList = await GetInputList(isPhrase, inputSource, client, reportOutput, allIntents, contentProvider, doContentCheck);
            _total = inputList.Count;
            foreach (var input in inputList)
            {
                await analyseBlock.SendAsync(input);
            }

            analyseBlock.Complete();
            await showResultBlock.Completion;

            _logger.LogDebug("TERMINOU!");

        }


        #region DataFlow Block Methods
        private async Task<NLPAnalyseDataBlock> AnalyseForMetrics(NLPAnalyseDataBlock dataBlock)
        {
            var response = await dataBlock.AIClient.AnalyseForMetrics(dataBlock.Input);
            dataBlock.NLPAnalysisResponse = response;
            return dataBlock;
        }

        private NLPAnalyseDataBlock CheckResponse(NLPAnalyseDataBlock dataBlock)
        {
            var item = dataBlock.NLPAnalysisResponse;
            if (item == null)
            {
                _logger.LogError($"Error when analysing: \"{dataBlock.Input}\"");
                return dataBlock;
            }
            return dataBlock;
        }

        private async Task<NLPAnalyseDataBlock> GetContent(NLPAnalyseDataBlock dataBlock)
        {
            if (dataBlock.DoContentCheck)
            {
                var intentId = dataBlock.NLPAnalysisResponse.Intentions?[0].Id;
                var intentName = dataBlock.AllIntents.FirstOrDefault(i => i.Id == intentId)?.Name;
                var entites = dataBlock.NLPAnalysisResponse.Entities?.Select(e => e.Value).ToList();
                dataBlock.ContentFromProvider = await dataBlock.ContentProvider.GetAsync(intentName, entites);
            }
            return dataBlock;
        }

        private async Task BuildResult(NLPAnalyseDataBlock dataBlock)
        {
            lock(_locker)
            {
                _count++;
                if(_count % 100 == 0)
                {
                    _logger.LogDebug($"{_count}/{_total}");
                }
            }

            var input = dataBlock.Input;
            var analysis = dataBlock.NLPAnalysisResponse;
            var content = dataBlock.ContentFromProvider;

            if (analysis == null)
                return;

            var resultData = new NLPAnalyseReportDataLine
            {
                Id = dataBlock.Id,
                Input = input,
                Intent = analysis.Intentions?[0].Id,
                Confidence = analysis.Intentions?[0].Score,
                Entities = analysis.Entities?.ToList().ToReportString(),
            };

            if (content != null)
            {
                resultData.Answer = ExtractAnswer(content);
            }

            var report = new NLPAnalyseReport
            {
                ReportDataLines = new List<NLPAnalyseReportDataLine> { resultData },
                FullReportFileName = dataBlock.ReportOutputFile
            };

            await _fileService.WriteAnalyseReportAsync(report, true);

            _logger.LogTrace($"\"{resultData.Input}\"\t{resultData.Intent}:{resultData.Confidence:P}\t{resultData.Entities}\t{CropText(resultData.Answer, 50)}");
        }
        #endregion

        private async Task<List<NLPAnalyseDataBlock>> GetInputList(bool isPhrase, string inputSource, IBlipAIClient client, string reportOutput, List<Intention> intentions, IContentProvider provider, bool doContentCheck)
        {
            if (isPhrase)
            {
                return new List<NLPAnalyseDataBlock> { NLPAnalyseDataBlock.GetInstance(1, inputSource, client, reportOutput, doContentCheck, intentions, provider) };
            }
            else
            {
                var inputListAsString = await _fileService.GetInputsToAnalyseAsync(inputSource);
                return inputListAsString
                    .Select((s, i) => NLPAnalyseDataBlock.GetInstance(i + 1, s, client, reportOutput, doContentCheck, intentions, provider))
                    .ToList();
            }
        }

        private string ExtractAnswer(ContentResult content)
        {
            return content.Status == ContentResultStatus.NotMatch ? "NotMatch" : GetContentText(content);
        }

        private string GetContentText(ContentResult content)
        {
            var text = content.Contents.FirstOrDefault().ContentText;
            text = Regex.Replace(text, "[\n\r]+", " ", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            text = Regex.Replace(text, "\\s+", " ", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            return text;
        }

        private string CropText(string text, int size)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            if (text.Length >= size)
                return $"{text.Substring(0, size - 1)}[...]";
            else
                return text;
        }

    }


}
