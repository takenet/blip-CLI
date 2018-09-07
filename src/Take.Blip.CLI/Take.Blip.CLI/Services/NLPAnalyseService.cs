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

        public NLPAnalyseService(
            IBlipClientFactory blipClientFactory,
            IFileManagerService fileService,
            IInternalLogger logger)
        {
            _blipClientFactory = blipClientFactory;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task AnalyseAsync(string authorization, string inputSource, string reportOutput)
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

            var resultDataList = new List<AnalysisResultData>();

            string EntitiesToString(List<EntityResponse> entities)
            {
                if (entities == null || entities.Count < 1)
                    return string.Empty;
                var toString = string.Join(", ", entities.Select(e => $"{e.Id}:{e.Value}"));
                return toString;
            }

            #region Methods
            async Task<(string, AnalysisResponse)> AnalyseForMetrics(string request)
            {
                var response = await client.AnalyseForMetrics(request);
                return (request, response);
            }

            (string, AnalysisResponse) CheckResponse((string request, AnalysisResponse response) tuple)
            {
                var item = tuple.response;
                if (item == null)
                {
                    _logger.LogError($"Error when analysing: \"{tuple.request}\"");
                    return tuple;
                }
                return tuple;
            }

            async Task<(string, AnalysisResponse, ContentResult)> GetContent((string request, AnalysisResponse response) tuple)
            {
                var intentId = tuple.response.Intentions?[0].Id;
                var intentName = allIntents.FirstOrDefault(i => i.Id == intentId)?.Name;
                var entites = tuple.response.Entities?.Select(e => e.Value).ToList();
                var content = await contentProvider.GetAsync(intentName, entites);

                return (tuple.request, tuple.response, content);
            }

            void BuildResult((string r, AnalysisResponse a, ContentResult c) tuple)
            {
                var input = tuple.r;
                var analysis = tuple.a;
                var content = tuple.c;

                if (analysis == null)
                    return;

                var resultData = new AnalysisResultData
                {
                    Input = input,
                    Intent = analysis.Intentions?[0].Id,
                    Confidence = analysis.Intentions?[0].Score,
                    Entities = EntitiesToString(analysis.Entities?.ToList()),
                };

                if (content != null)
                {
                    resultData.Answer = ExtractAnswer(content);
                }

                resultDataList.Add(resultData);

                _logger.LogTrace($"\"{resultData.Input}\"\t{resultData.Intent}:{resultData.Confidence:P}\t{resultData.Entities}\t{CropText(resultData.Answer, 50)}");
            }
            #endregion

            var options = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = DataflowBlockOptions.Unbounded,
                MaxDegreeOfParallelism = 20
            };

            var analyseBlock = new TransformBlock<string, (string, AnalysisResponse)>((Func<string, Task<(string, AnalysisResponse)>>)AnalyseForMetrics, options);
            var checkBlock = new TransformBlock<(string, AnalysisResponse), (string, AnalysisResponse)>((Func<(string, AnalysisResponse), (string, AnalysisResponse)>)CheckResponse, options);
            var contentBlock = new TransformBlock<(string, AnalysisResponse), (string, AnalysisResponse, ContentResult)>((Func<(string, AnalysisResponse), Task<(string, AnalysisResponse, ContentResult)>>)GetContent, options);
            var showResultBlock = new ActionBlock<(string, AnalysisResponse, ContentResult)>((Action<(string, AnalysisResponse, ContentResult)>)BuildResult, options);

            analyseBlock.LinkTo(checkBlock, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });
            checkBlock.LinkTo(contentBlock, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });
            contentBlock.LinkTo(showResultBlock, new DataflowLinkOptions
            {
                PropagateCompletion = true
            });


            var inputList = await GetInputList(isPhrase, inputSource);
            List<string> elements100;
            int chunkSize = 5;
            int skip = 0;
            int total = 0;

            while ((elements100 = inputList.Skip(skip).Take(chunkSize).ToList()).Any())
            {
                skip += chunkSize;
                total += elements100.Count;
                _logger.LogDebug($"\t\tProcessando {elements100.Count} - {total}/{inputList.Count}");

                resultDataList.Clear();

                foreach (var input in elements100)
                {
                    await analyseBlock.SendAsync(input);
                }

                analyseBlock.Complete();
                await showResultBlock.Completion;

                var report = new NLPAnalyseReport
                {
                    ResultData = resultDataList,
                    FullReportFileName = reportOutput
                };

                await _fileService.WriteAnalyseReportAsync(report, true);
                
            }

            _logger.LogDebug("TERMINOU!");

        }

        private async Task<List<string>> GetInputList(bool isPhrase, string inputSource)
        {
            if (isPhrase)
            {
                return new List<string> { inputSource };
            }
            else
            {
                return await _fileService.GetInputsToAnalyseAsync(inputSource);
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
            if (text.Length >= size)
                return $"{text.Substring(0, size - 1)}[...]";
            else
                return text;
        }

    }


}
