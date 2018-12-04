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
using Take.BlipCLI.Models.NLPAnalyse;
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
        private const string BOT_KEY_PREFIX = "BotKey:";
        private readonly IBlipClientFactory _blipClientFactory;
        private readonly IFileManagerService _fileService;
        private readonly IJitterService _jitterService;
        private readonly IInternalLogger _logger;

        private static object _locker = new object();
        private static int _count = 0;
        private static int _total = 0;

        public NLPAnalyseService(
            IBlipClientFactory blipClientFactory,
            IFileManagerService fileService,
            IJitterService jitterService,
            IInternalLogger logger)
        {
            _blipClientFactory = blipClientFactory;
            _fileService = fileService;
            _jitterService = jitterService;
            _logger = logger;
        }

        public async Task AnalyseAsync(AnalyseParameters analyseParameters)
        {
            var authorization = analyseParameters.Authorization;
            var inputSource = analyseParameters.InputSource;
            var reportOutput = analyseParameters.ReportOutput;
            var doContentCheck = analyseParameters.DoContentCheck;
            var rawContent = analyseParameters.ShowRawContent;
            var jitterSize = analyseParameters.JitterSize;

            if (string.IsNullOrEmpty(authorization))
                throw new ArgumentNullException("You must provide the target bot (node) for this action.");

            if (string.IsNullOrEmpty(inputSource))
                throw new ArgumentNullException("You must provide the input source (phrase or file) for this action.");

            if (string.IsNullOrEmpty(reportOutput))
                throw new ArgumentNullException("You must provide the full output's report file name for this action.");

            _logger.LogDebug("COMEÇOU!");

            _fileService.CreateDirectoryIfNotExists(reportOutput);

            var client = _blipClientFactory.GetInstanceForAI(authorization);
            IContentManagerApiClient contentClient = new ContentManagerApiClient(authorization);
            var allIntents = new List<Intention>();
            if (doContentCheck)
            {
                _logger.LogDebug("\tCarregando intencoes...");
                allIntents = await client.GetAllIntentsAsync();
                _logger.LogDebug("\tCarregadas!");
            }

            var inputType = InputType.Phrase;
            inputType = DetectInputType(inputSource);

            var options = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = DataflowBlockOptions.Unbounded,
                MaxDegreeOfParallelism = 20,

            };

            var analyseBlock = new TransformBlock<DataBlock, DataBlock>((Func<DataBlock, Task<DataBlock>>)AnalyseForMetrics, options);
            var checkBlock = new TransformBlock<DataBlock, DataBlock>((Func<DataBlock, DataBlock>)CheckResponse, options);
            var contentBlock = new TransformBlock<DataBlock, DataBlock>((Func<DataBlock, Task<DataBlock>>)GetContent, options);
            var buildResultBlock = new ActionBlock<DataBlock>(BuildResult, new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = DataflowBlockOptions.Unbounded,
                MaxMessagesPerTask = 1
            });

            analyseBlock.LinkTo(checkBlock, new DataflowLinkOptions { PropagateCompletion = true });
            checkBlock.LinkTo(contentBlock, new DataflowLinkOptions { PropagateCompletion = true });
            contentBlock.LinkTo(buildResultBlock, new DataflowLinkOptions { PropagateCompletion = true });

            _count = 0;

            var dataBlockInputList = await GetInputList(inputType, inputSource, client, contentClient, reportOutput, allIntents, doContentCheck, rawContent);

            var jitteredInputStrings = new HashSet<string>();
            var distribution = JitterDistribution.NewDistribution(0.5f, 0.1f, 0.3f, 0.1f);
            foreach (var dbi in dataBlockInputList)
            {
                var inputText = dbi.Input.Input;
                jitteredInputStrings.Add($"{dbi.Id}_SEP_{inputText}");
                for (int i = 0; i < jitterSize; i++)
                {
                    var jitteredInput = await _jitterService.ApplyJitterAsync(inputText, distribution, 0.8f);
                    jitteredInputStrings.Add($"{dbi.Id}_SEP_{jitteredInput}");
                }
            }

            var dataBlockExpandedInputList = jitteredInputStrings
                .Select((s) =>
                {
                    var parts = s.Split("_SEP_");
                    return DataBlock.GetInstance(int.Parse(parts[0]), InputWithTags.FromText(parts[1]), client, contentClient, reportOutput, doContentCheck, rawContent, allIntents);
                })
                .ToList();

            if (!dataBlockExpandedInputList.Any())
            {
                dataBlockExpandedInputList = dataBlockInputList;
            }

            _total = dataBlockExpandedInputList.Count;
            foreach (var input in dataBlockExpandedInputList)
            {
                await analyseBlock.SendAsync(input);
            }

            analyseBlock.Complete();
            await buildResultBlock.Completion;

            _logger.LogDebug("TERMINOU!");

        }

        #region DataFlow Block Methods
        private async Task<DataBlock> AnalyseForMetrics(DataBlock dataBlock)
        {
            var response = await dataBlock.AIClient.AnalyseForMetrics(dataBlock.Input.Input);
            dataBlock.NLPAnalysisResponse = response;
            return dataBlock;
        }

        private DataBlock CheckResponse(DataBlock dataBlock)
        {
            var item = dataBlock.NLPAnalysisResponse;
            if (item == null)
            {
                _logger.LogError($"Error when analysing: \"{dataBlock.Input}\"");
                return dataBlock;
            }
            return dataBlock;
        }

        private async Task<DataBlock> GetContent(DataBlock dataBlock)
        {
            if (dataBlock.DoContentCheck)
            {
                var intentId = dataBlock.NLPAnalysisResponse.Intentions?[0].Id;
                var intentName = dataBlock.AllIntents.FirstOrDefault(i => i.Id == intentId)?.Name;
                var entities = dataBlock.NLPAnalysisResponse.Entities?.Select(e => e.Value).ToList();
                dataBlock.ContentFromProvider = await dataBlock.ContentClient.GetAnswerAsync(intentName, entities, dataBlock.Input.Tags);
            }
            return dataBlock;
        }

        private async Task BuildResult(DataBlock dataBlock)
        {
            lock (_locker)
            {
                _count++;
                if (_count % 100 == 0)
                {
                    _logger.LogDebug($"{_count}/{_total}");
                }
            }

            try
            {
                var input = dataBlock.Input;
                var analysis = dataBlock.NLPAnalysisResponse;
                var content = dataBlock.ContentFromProvider;

                if (analysis == null)
                    return;

                var resultData = new ReportDataLine
                {
                    Id = dataBlock.Id,
                    Input = input,
                    Intent = analysis.Intentions?[0].Id,
                    Confidence = analysis.Intentions?[0].Score,
                    Entities = analysis.Entities?.ToList().ToReportString(),
                    AnalysisResponse = dataBlock.NLPAnalysisResponse
                };

                if (content != null)
                {
                    resultData.Answer = ExtractAnswer(content);
                }

                var report = new Report
                {
                    ReportDataLines = new List<ReportDataLine> { resultData },
                    FullReportFileName = dataBlock.ReportOutputFile,
                    WriteRawContent = dataBlock.ShouldWriteRawContent
                };

                await _fileService.WriteAnalyseReportAsync(report, true);
                _logger.LogTrace($"\"{resultData.Input.Input}\"\t{resultData.Intent}:{resultData.Confidence:P}\t{resultData.Entities}\t{CropText(resultData.Answer, 50)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error BuildResult for {dataBlock}");
                throw ex;
            }

        }
        #endregion

        private async Task<List<DataBlock>> GetInputList(
            InputType inputType,
            string inputSource,
            IBlipAIClient client,
            IContentManagerApiClient contentClient,
            string reportOutput,
            List<Intention> intentions,
            bool doContentCheck,
            bool rawContent)
        {
            switch (inputType)
            {
                case InputType.Phrase:
                    return new List<DataBlock> { DataBlock.GetInstance(1, InputWithTags.FromText(inputSource), client, contentClient, reportOutput, doContentCheck, rawContent, intentions) };
                case InputType.File:
                    var inputListAsString = await _fileService.GetInputsToAnalyseAsync(inputSource);
                    return inputListAsString
                        .Select((input, i) => DataBlock.GetInstance(i + 1, input, client, contentClient, reportOutput, doContentCheck, rawContent, intentions))
                        .ToList();
                case InputType.Bot:
                    var botSource = inputSource.Replace(BOT_KEY_PREFIX, "").Trim();
                    var localClient = _blipClientFactory.GetInstanceForAI(botSource);
                    _logger.LogDebug("\tCarregando intenções do bot fonte...");
                    var allIntents = await localClient.GetAllIntentsAsync();
                    var questionListAsString = new List<string>();
                    foreach (var intent in allIntents)
                    {
                        questionListAsString.AddRange(intent.Questions.Select(q => q.Text));
                    }
                    _logger.LogDebug("\tIntenções carregadas!");
                    return questionListAsString
                        .Select((input, i) => DataBlock.GetInstance(i + 1, InputWithTags.FromText(input), client, contentClient, reportOutput, doContentCheck, rawContent, intentions))
                        .ToList();
                default:
                    throw new ArgumentException($"Unexpected value {inputType}.", "inputType");
            }

        }

        private string ExtractAnswer(ContentManagerContentResult content)
        {
            //Status = 0 -> ExactMatch
            return content.Status == 0 ? GetContentText(content) : "NotMatch";
        }

        private string GetContentText(ContentManagerContentResult content)
        {
            var text = content.Contents.FirstOrDefault().ContentText;
            if (string.IsNullOrEmpty(text)) return text;
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

        private InputType DetectInputType(string inputSource)
        {
            InputType inputType;
            var isDirectory = _fileService.IsDirectory(inputSource);
            var isFile = _fileService.IsFile(inputSource);

            if (isFile)
            {
                _logger.LogDebug("\tA entrada é um arquivo");
                inputType = InputType.File;
            }
            else
            if (isDirectory)
            {
                _logger.LogError("\tA entrada é um diretório");
                throw new ArgumentNullException("You must provide the input source (phrase or file) for this action. Your input was a direcory.");
            }
            else
            {
                if (inputSource.StartsWith(BOT_KEY_PREFIX))
                {
                    _logger.LogDebug("\tA entrada é um bot");
                    inputType = InputType.Bot;
                }
                else
                {
                    _logger.LogDebug("\tA entrada é uma frase");
                    inputType = InputType.Phrase;
                }

            }

            return inputType;
        }



    }


}
