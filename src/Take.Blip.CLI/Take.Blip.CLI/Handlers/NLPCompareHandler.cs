using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITGlobal.CommandLine;
using Lime.Messaging.Contents;
using Microsoft.Extensions.Logging;
using Take.BlipCLI.Models;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;
using Take.BlipCLI.Services.TextSimilarity.Interfaces;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Handlers
{
    public class NLPCompareHandler : HandlerAsync
    {
        private readonly IBlipClientFactory _blipClientFactory;
        private readonly ITextSimilarityServiceFactory _textSimilarityServiceFactory;

        public INamedParameter<string> Authorization1 { get; internal set; }
        public INamedParameter<string> Authorization2 { get; internal set; }
        public INamedParameter<string> OutputFilePath { get; internal set; }
        public INamedParameter<ComparisonMethod> Method { get; internal set; }
        public INamedParameter<string> Bot1Path { get; internal set; }
        public INamedParameter<string> Bot2Path { get; internal set; }
        public bool IsVerbose { get => Verbose.IsSet; }

        public NLPCompareHandler(
            IInternalLogger logger,
            IBlipClientFactory blipClientFactory,
            ITextSimilarityServiceFactory textSimilarityServiceFactory
            ) : base(logger)
        {
            _blipClientFactory = blipClientFactory;
            _textSimilarityServiceFactory = textSimilarityServiceFactory;
        }

        public override async Task<int> RunAsync(string[] args)
        {
            if (!Authorization1.IsSet && !Bot1Path.IsSet)
                throw new ArgumentNullException("You must provide a first bot parameter for this action. Use '--a1' [--first] (authorizationKey) or '--p1' [--path1] (path to an exported model) parameters");

            bool internalComparison = false;
            if (!Authorization2.IsSet && !Bot2Path.IsSet)
            {
                _logger.LogWarning("Comparing model with itsef. If you want to compare between models, you must provide a second bot parameter. Use '--a2' [--second] (authorizationKey) or '--p2' [--path2] (path to an exported model) parameters");
                internalComparison = true;
                //throw new ArgumentNullException("You must provide a second bot parameter for this action. Use '--a2' [--second] (authorizationKey) or '--p2' [--path2] (path to an exported model) parameters");
            }

            if (!OutputFilePath.IsSet)
                throw new ArgumentNullException("You must provide output file path parameter for this action. Use -o [--output] parameter.");

            NLPModel bot1Model = Bot1Path.IsSet ? GetBotModelFromPath(Bot1Path.Value) : await GetBotModelFromAPI(Authorization1.Value);
            NLPModel bot2Model = internalComparison ? bot1Model : (Bot2Path.IsSet ? GetBotModelFromPath(Bot2Path.Value) : await GetBotModelFromAPI(Authorization2.Value));

            var textSimService = _textSimilarityServiceFactory.GetServiceByType(Method.Value);

            var report = new List<NLPModelComparationResult>();
            _logger.LogInformation("Comparing questions");
            CompareIntentionsByQuestions(textSimService, bot1Model, bot2Model, report, internalComparison);
            _logger.LogInformation("Comparing answers");
            CompareIntentionsByAnswers(textSimService, bot1Model, bot2Model, report, internalComparison);
            _logger.LogInformation("Comparing names");
            CompareIntentionsByName(textSimService, bot1Model, bot2Model, report, internalComparison);

            _logger.LogInformation("Writing report");
            var now = DateTime.Now;

            var fileFullName = $"report_{now.ToString("yyyyMMdd_hhmm")}.txt";
            Directory.CreateDirectory(OutputFilePath.Value);
            using (var sw = new StreamWriter(Path.Combine(OutputFilePath.Value, fileFullName)))
            {
                foreach (var result in report)
                {
                    sw.WriteLine($"Maybe intention \"{result.Element1}\" is close to intention \"{result.Element2}\"");
                    sw.WriteLine($"Because: ");
                    foreach (var reason in result.Reasons)
                    {
                        foreach (var example in reason.Examples)
                        {
                            sw.WriteLine($"\t{Enum.GetName(typeof(NLPModelComparationType), reason.Reason)}:\t{example.Text}");
                        }
                    }
                }
            }
            _logger.LogInformation("Finish");
            return 0;
        }

        private void CompareIntentionsByName(
            ITextSimilarityService textSimilarityService,
            NLPModel bot1Model,
            NLPModel bot2Model,
            List<NLPModelComparationResult> report,
            bool internalComparison)
        {
            foreach (var item1 in bot1Model.Intents)
            {
                foreach (var item2 in bot2Model.Intents)
                {

                    var text1 = item1.Name;
                    var text2 = item2.Name;
                    var name1 = item1.Name;
                    var name2 = item2.Name;
                    if (internalComparison && name1.Equals(name2)) continue;
                    CompareTextsToReport(textSimilarityService, report, text1, text2, name1, name2, NLPModelComparationType.Name);
                }
            }
        }

        private void CompareIntentionsByAnswers(
            ITextSimilarityService textSimilarityService,
            NLPModel bot1Model,
            NLPModel bot2Model,
            List<NLPModelComparationResult> report,
            bool internalComparison)
        {
            foreach (var item1 in bot1Model.Intents)
            {
                if (item1.Answers == null) continue;
                foreach (var item2 in bot2Model.Intents)
                {
                    if (item2.Answers == null) continue;
                    if (internalComparison && item1.Name.Equals(item2.Name)) continue;
                    foreach (var answer1 in item1.Answers)
                    {
                        foreach (var answer2 in item2.Answers)
                        {
                            var text1 = answer1.Value.ToString();
                            var text2 = answer2.Value.ToString();
                            var name1 = item1.Name;
                            var name2 = item2.Name;
                            CompareTextsToReport(textSimilarityService, report, text1, text2, name1, name2, NLPModelComparationType.Answer);
                        }
                    }
                }
            }
        }

        private void CompareIntentionsByQuestions(
            ITextSimilarityService textSimilarityService,
            NLPModel bot1Model,
            NLPModel bot2Model,
            List<NLPModelComparationResult> report,
            bool internalComparison)
        {
            int count = 0;
            foreach (var item1 in bot1Model.Intents)
            {
                if (item1.Questions == null) continue;
                foreach (var item2 in bot2Model.Intents)
                {
                    if (item2.Questions == null) continue;
                    count += item1.Questions.Length * item2.Questions.Length;
                }
            }
            _logger.LogDebug($"{count} question pairs to analyse.");
            int analyzed = 0;
            int slot = count < 1000 ? 100 : count / 20;
            foreach (var item1 in bot1Model.Intents)
            {
                if (item1.Questions == null) continue;
                foreach (var item2 in bot2Model.Intents)
                {
                    if (item2.Questions == null) continue;
                    foreach (var question1 in item1.Questions)
                    {
                        foreach (var question2 in item2.Questions)
                        {
                            analyzed++;
                            if(analyzed % slot == 0) { _logger.LogDebug($"{analyzed}/{count} analysed."); }
                            var text1 = question1.Text;
                            var text2 = question2.Text;
                            var name1 = item1.Name;
                            var name2 = item2.Name;
                            if (internalComparison && text1.Equals(text2)) continue;
                            CompareTextsToReport(textSimilarityService, report, text1, text2, name1, name2,
                                internalComparison && item1.Name == item2.Name ?
                                NLPModelComparationType.QuestionInSameIntent :
                                NLPModelComparationType.Question);
                        }
                    }
                }
            }
        }

        private void CompareTextsToReport(
            ITextSimilarityService textSimilarityService,
            List<NLPModelComparationResult> report,
            string text1, string text2,
            string key1, string key2,
            NLPModelComparationType criterion)
        {
            var distance = textSimilarityService.CalculateDistance(text1, text2);
            var minimunDistance = textSimilarityService.CalculateMinimumDistance(text1, text2, criterion);
            if (distance <= minimunDistance)
            {
                var result = report.FirstOrDefault(r => r.CheckKey(key1, key2));
                if (result == default(NLPModelComparationResult))
                {
                    report.Add(new NLPModelComparationResult
                    {
                        Element1 = key1,
                        Element2 = key2,
                        Reasons = new List<NLPModelComparationResultReason>()
                    });
                    result = report.First(r => r.CheckKey(key1, key2));
                }

                var reason = result.Reasons.FirstOrDefault(r => r.Reason == criterion);
                if (reason == default(NLPModelComparationResultReason))
                {
                    result.Reasons.Add(new NLPModelComparationResultReason
                    {
                        Reason = criterion,
                        Examples = new List<NLPModelComparationResultReasonExample>()
                    });
                    reason = result.Reasons.First(r => r.Reason == criterion);
                }
                if (!reason.Examples.Any(e => e.CheckKey(text1, text2)))
                {
                    reason.Examples.Add(new NLPModelComparationResultReasonExample
                    {
                        Key1 = text1,
                        Key2 = text2,
                        Text = $"Dist(\"{text1}\",\"{text2}\") = {distance}, min = {minimunDistance}"
                    });
                }
            }
        }



        private async Task<NLPModel> GetBotModelFromAPI(string authKey)
        {
            IBlipAIClient blipAIClient = _blipClientFactory.GetInstanceForAI(authKey);

            var entities = await blipAIClient.GetAllEntities(verbose: IsVerbose);
            var intents = await blipAIClient.GetAllIntentsAsync(verbose: IsVerbose);

            return new NLPModel { BotId = authKey, Entities = entities, Intents = intents };
        }

        private NLPModel GetBotModelFromPath(string path)
        {
            var intentinsQuestionsMap = new Dictionary<string, List<string>>();
            var intentinsAnswersMap = new Dictionary<string, List<string>>();

            var intentCsv = new Chilkat.Csv { HasColumnNames = true };
            var entityCsv = new Chilkat.Csv { HasColumnNames = true };
            var answerCsv = new Chilkat.Csv { HasColumnNames = true };

            var intentPath = Path.Combine(path, "intentions.csv");
            var entityPath = Path.Combine(path, "entities.csv");
            var answerPath = Path.Combine(path, "answers.csv");

            bool loadedIntent = intentCsv.LoadFile(intentPath);
            bool loadedEntity = entityCsv.LoadFile(entityPath);
            bool loadedAnswer = answerCsv.LoadFile(answerPath);

            if (!loadedAnswer || !loadedEntity || !loadedIntent)
            {
                return null;
            }

            var model = new NLPModel
            {
                BotId = path,
                Entities = new List<Entity>(),
                Intents = new List<Intention>()
            };

            for (int row = 0; row <= intentCsv.NumRows - 1; row++)
            {
                var intentionName = intentCsv.GetCell(row, 0);
                var question = intentCsv.GetCell(row, 1);

                var questionsList = intentinsQuestionsMap.ContainsKey(intentionName) ? intentinsQuestionsMap[intentionName] : new List<string>();

                questionsList.Add(question);
                intentinsQuestionsMap[intentionName] = questionsList;
            }

            for (int row = 0; row <= answerCsv.NumRows - 1; row++)
            {
                var intentionName = answerCsv.GetCell(row, 0);
                var answer = answerCsv.GetCell(row, 1);

                var answerList = intentinsAnswersMap.ContainsKey(intentionName) ? intentinsAnswersMap[intentionName] : new List<string>();

                answerList.Add(answer);
                intentinsAnswersMap[intentionName] = answerList;
            }

            foreach (var intent in intentinsQuestionsMap)
            {
                model.Intents.Add(
                    new Intention
                    {
                        Name = intent.Key,
                        Questions = intent.Value.Select(q =>
                            new Question
                            {
                                Text = q
                            }).ToArray()
                    });
            }

            foreach (var intent in intentinsAnswersMap)
            {
                var existedIntent = model.Intents.FirstOrDefault(i => i.Name.Equals(intent.Key));
                if (existedIntent == default(Intention))
                    continue;
                existedIntent.Answers = intent.Value.Select(a =>
                            new Answer
                            {
                                Value = PlainText.Parse(a),
                                Type = PlainText.MediaType
                            }).ToArray();
            }


            return model;
        }

        internal ComparisonMethod CustomMethodParser(string arg)
        {
            var defaultMethod = ComparisonMethod.Exact;

            if (string.IsNullOrWhiteSpace(arg)) return defaultMethod;

            var getType = TryGetContentType(arg);
            if (getType.HasValue)
            {
                return getType.Value;
            }

            return defaultMethod;
        }

        private ComparisonMethod? TryGetContentType(string content)
        {
            var validContents = Enum.GetNames(typeof(ComparisonMethod));
            var validContent = validContents.FirstOrDefault(c => c.ToLowerInvariant().Equals(content.ToLowerInvariant()));

            if (validContent != null)
                return Enum.Parse<ComparisonMethod>(validContent);

            return null;
        }
    }

    public enum ComparisonMethod
    {
        Exact,
        Levenshtein,
        JaroWinklerAndConsine
    }

}
