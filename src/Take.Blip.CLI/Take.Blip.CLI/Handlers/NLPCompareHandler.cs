using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITGlobal.CommandLine;
using Lime.Messaging.Contents;
using Take.BlipCLI.Models;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Handlers
{
    public class NLPCompareHandler : HandlerAsync
    {
        private readonly IStringService _stringService;

        public INamedParameter<string> Authorization1 { get; internal set; }
        public INamedParameter<string> Authorization2 { get; internal set; }
        public INamedParameter<string> OutputFilePath { get; internal set; }
        public INamedParameter<ComparisonMethod> Method { get; internal set; }
        public INamedParameter<string> Bot1Path { get; internal set; }
        public INamedParameter<string> Bot2Path { get; internal set; }
        public bool IsVerbose { get => Verbose.IsSet; }

        public NLPCompareHandler(IStringService stringService)
        {
            this._stringService = stringService;
        }

        public override async Task<int> RunAsync(string[] args)
        {
            if (!Authorization1.IsSet && !Bot1Path.IsSet)
                throw new ArgumentNullException("You must provide a first bot parameter for this action. Use '--a1' [--first] (authorizationKey) or '--p1' [--path1] (path to an exported model) parameters");

            if (!Authorization2.IsSet && !Bot2Path.IsSet)
                throw new ArgumentNullException("You must provide a second bot parameter for this action. Use '--a2' [--second] (authorizationKey) or '--p2' [--path2] (path to an exported model) parameters");

            if (!OutputFilePath.IsSet)
                throw new ArgumentNullException("You must provide output file path parameter for this action. Use -o [--output] parameter.");

            NLPModel bot1Model = Bot1Path.IsSet ? GetBotModelFromPath(Bot1Path.Value) : await GetBotModelFromAPI(Authorization1.Value);
            NLPModel bot2Model = Bot2Path.IsSet ? GetBotModelFromPath(Bot2Path.Value) : await GetBotModelFromAPI(Authorization2.Value);



            var report = new List<NLPModelComparationResult>();
            CompareIntentionsByQuestions(bot1Model, bot2Model, report);
            CompareIntentionsByAnswers(bot1Model, bot2Model, report);
            CompareIntentionsByName(bot1Model, bot2Model, report);

            var now = DateTime.Now;

            var fileFullName = $"report_{now.ToString("yyyyMMdd_hhmm")}.txt";
            Directory.CreateDirectory(OutputFilePath.Value);
            var sw = new StreamWriter(Path.Combine(OutputFilePath.Value, fileFullName));
            foreach (var result in report)
            {
                sw.WriteLine($"Intention \"{result.Element1}\" is close to \"{result.Element2}\"");
                sw.WriteLine($"Because: ");
                foreach (var reason in result.Reasons)
                {
                    foreach (var example in reason.Examples)
                    {
                        sw.WriteLine($"\t{Enum.GetName(typeof(NLPModelComparationResultReasonType), reason.Reason)}:\t{example}");
                    }
                }
            }
            sw.Close();


            return 0;
        }

        private void CompareIntentionsByName(NLPModel bot1Model, NLPModel bot2Model, List<NLPModelComparationResult> report)
        {
            foreach (var item1 in bot1Model.Intents)
            {
                foreach (var item2 in bot2Model.Intents)
                {
                    var text1 = item1.Name;
                    var text2 = item2.Name;
                    var name1 = item1.Name;
                    var name2 = item2.Name;
                    CompareTextsToReport(report, text1, text2, name1, name2, NLPModelComparationResultReasonType.Name);
                }
            }
        }

        private void CompareIntentionsByAnswers(NLPModel bot1Model, NLPModel bot2Model, List<NLPModelComparationResult> report)
        {
            foreach (var item1 in bot1Model.Intents)
            {
                if (item1.Answers == null) continue;
                foreach (var item2 in bot2Model.Intents)
                {
                    if (item2.Answers == null) continue;
                    foreach (var answer1 in item1.Answers)
                    {
                        foreach (var answer2 in item2.Answers)
                        {
                            var text1 = answer1.Value.ToString();
                            var text2 = answer2.Value.ToString();
                            var name1 = item1.Name;
                            var name2 = item2.Name;
                            CompareTextsToReport(report, text1, text2, name1, name2, NLPModelComparationResultReasonType.Answer);
                        }
                    }
                }
            }
        }

        private void CompareIntentionsByQuestions(NLPModel bot1Model, NLPModel bot2Model, List<NLPModelComparationResult> report)
        {
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
                            var text1 = question1.Text;
                            var text2 = question2.Text;
                            var name1 = item1.Name;
                            var name2 = item2.Name;
                            CompareTextsToReport(report, text1, text2, name1, name2, NLPModelComparationResultReasonType.Question);
                        }
                    }
                }
            }
        }

        private void CompareTextsToReport(List<NLPModelComparationResult> report, string text1, string text2, string key1, string key2, NLPModelComparationResultReasonType criterion)
        {
            int distance = _stringService.LevenshteinDistance(text1, text2);
            int minimunLeveshteinDistance = CalculateMinimumLeveshteinDistance(text1, text2);
            if (distance <= minimunLeveshteinDistance)
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
                        Examples = new List<string>()
                    });
                    reason = result.Reasons.First(r => r.Reason == criterion);
                }
                reason.Examples.Add($"Dist(\"{text1}\",\"{text2}\") = {distance}, min = {minimunLeveshteinDistance}");
            }
        }

        private int CalculateMinimumLeveshteinDistance(string v1, string v2)
        {
            int smallerStringSize = Math.Min(v1.Length, v2.Length);
            return (int)Math.Max(1, 2 * Math.Log(smallerStringSize));
        }

        private async Task<NLPModel> GetBotModelFromAPI(string authKey)
        {
            IBlipAIClient blipAIClient = new BlipHttpClientAsync(authKey);

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
        Levenshtein
    }

}
