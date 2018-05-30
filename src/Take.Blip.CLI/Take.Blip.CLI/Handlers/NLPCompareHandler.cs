using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITGlobal.CommandLine;
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
        public ISwitch Verbose { get; internal set; }
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


            foreach (var item1 in bot1Model.Intents)
            {
                if (item1.Questions == null) continue;
                foreach (var question1 in item1.Questions)
                {
                    foreach (var item2 in bot2Model.Intents)
                    {
                        if (item2.Questions == null) continue;
                        foreach (var question2 in item2.Questions)
                        {
                            int distance = _stringService.LevenshteinDistance(question1.Text, question2.Text);
                            if (distance <= 4)
                            {
                                Console.WriteLine($"{item1.Name} is close to {item2.Name}");
                                Console.WriteLine($"\tLev({question1.Text},{question2.Text}) = {distance}");
                            }
                        }
                    }
                }
            }
            return 0;
        }

        private async Task<NLPModel> GetBotModelFromAPI(string authKey)
        {
            IBlipAIClient blipAIClient = new BlipHttpClientAsync(authKey);

            var entities = await blipAIClient.GetAllEntities(verbose: IsVerbose);
            var intents = await blipAIClient.GetAllIntents(verbose: IsVerbose);

            return new NLPModel { BotId = authKey, Entities = entities, Intents = intents };
        }

        private NLPModel GetBotModelFromPath(string path)
        {
            var intentionsMap = new Dictionary<string, List<string>>();

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

                var questionsList = intentionsMap.ContainsKey(intentionName) ? intentionsMap[intentionName] : new List<string>();

                questionsList.Add(question);
                intentionsMap[intentionName] = questionsList;
            }

            foreach (var intent in intentionsMap)
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



            throw new NotImplementedException();
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
