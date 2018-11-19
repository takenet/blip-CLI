using ITGlobal.CommandLine;
using Lime.Messaging.Contents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;
using Take.BlipCLI.Services.Settings;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Handlers
{
    public class NLPImportHandler : HandlerAsync
    {
        public INamedParameter<string> Node { get; set; }
        public INamedParameter<string> Authorization { get; set; }
        public INamedParameter<string> IntentsFilePath { get; set; }
        public INamedParameter<string> EntitiesFilePath { get; set; }
        public INamedParameter<string> AnswersFilePath { get; set; }

        private readonly ISettingsFile _settingsFile;
        private readonly IBlipClientFactory _blipClientFactory;
        private IBlipAIClient _blipAIClient;

        public NLPImportHandler(
            IBlipClientFactory blipClientFactory, 
            IInternalLogger logger) : base(logger)
        {
            _settingsFile = new SettingsFile();
            _blipClientFactory = blipClientFactory;
        }

        public override async Task<int> RunAsync(string[] args)
        {
            if (!Node.IsSet && !Authorization.IsSet)
                throw new ArgumentNullException("You must provide the target bot (node) for this action. Use '-n' [--node] (or '-a' [--authorization]) parameters");

            string authorization = Authorization.Value;

            if (Node.IsSet)
            {
                authorization = _settingsFile.GetNodeCredentials(Lime.Protocol.Node.Parse(Node.Value)).Authorization;
            }

            _blipAIClient = _blipClientFactory.GetInstanceForAI(authorization);

            var intents = new List<Intention>();
            if (IntentsFilePath.IsSet)
            {
                Console.WriteLine("Starting intents import task...");
                intents = await ImportIntentions();
                Console.WriteLine("Intents imported with success...");
            }

            if (AnswersFilePath.IsSet && intents != null && intents.Count > 0)
            {
                Console.WriteLine("Starting answers import task...");
                await ImportAnswers(intents);
                Console.WriteLine("Answers imported with success...");
            }


            if (EntitiesFilePath.IsSet)
            {
                Console.WriteLine("Starting entities import task...");
                await ImportEntities();
                Console.WriteLine("Entities imported with success...");
            }

            return 0;
        }

        private async Task<List<Intention>> ImportIntentions()
        {
            var intentionsMap = new Dictionary<string, List<string>>();

            //Get intentions on file
            var csv = new Chilkat.Csv
            {
                //  Prior to loading the CSV file, indicate that the 1st row
                //  should be treated as column names:
                HasColumnNames = true
            };

            //  Load the CSV records from intentions the file:
            bool success = csv.LoadFile(IntentsFilePath.Value);
            if (!success)
            {
                Console.WriteLine(csv.LastErrorText);
                return null;
            }

            //  Display the contents of the 3rd column (i.e. the country names)
            for (int row = 0; row <= csv.NumRows - 1; row++)
            {
                var intentionName = csv.GetCell(row, 0);
                var question = csv.GetCell(row, 1);

                var questionsList = intentionsMap.ContainsKey(intentionName) ? intentionsMap[intentionName] : new List<string>();

                questionsList.Add(question);
                intentionsMap[intentionName] = questionsList;
            }

            var intents = new List<Intention>();

            //Add each intention on BLiP IA model
            foreach (var intentionKey in intentionsMap.Keys)
            {
                var id = await _blipAIClient.AddIntent(intentionKey);

                var questionsList = intentionsMap[intentionKey];
                var questionsArray = questionsList.Select(q => new Question { Text = q }).ToArray();

                await _blipAIClient.AddQuestions(id, questionsArray);

                intents.Add(new Intention { Id = id, Questions = questionsArray, Name = intentionKey });

            }

            return intents;
        }


        private async Task ImportAnswers(List<Intention> intentions)
        {
            var answersMap = new Dictionary<string, List<string>>();

            //Get intentions on file
            var csv = new Chilkat.Csv
            {
                //  Prior to loading the CSV file, indicate that the 1st row
                //  should be treated as column names:
                HasColumnNames = true
            };

            //  Load the CSV records from intentions the file:
            bool success = csv.LoadFile(AnswersFilePath.Value);
            if (!success)
            {
                Console.WriteLine(csv.LastErrorText);
                return;
            }

            //  Display the contents of the 3rd column (i.e. the country names)
            for (int row = 0; row <= csv.NumRows - 1; row++)
            {
                var intentionName = csv.GetCell(row, 0);
                var answer = csv.GetCell(row, 1);

                var answersList = answersMap.ContainsKey(intentionName) ? answersMap[intentionName] : new List<string>();

                answersList.Add(answer);
                answersMap[intentionName] = answersList;
            }

            var intents = new List<Intention>();

            //Add each intention on BLiP IA model
            foreach (var intentionKey in answersMap.Keys)
            {
                var intention = intentions.FirstOrDefault(i => i.Name == intentionKey);
                if (intention == null)
                {
                    Console.WriteLine($"{intentionKey} not present in intentions list.");
                    continue;
                }

                var answersList = answersMap[intentionKey];
                var answersArray = answersList.Select(q => new Answer { RawValue = q, Type = PlainText.MediaType, Value = PlainText.Parse(q) }).ToArray();

                await _blipAIClient.AddAnswers(intention.Id, answersArray);

            }

        }

        private async Task ImportEntities()
        {
            var entitiesMap = new Dictionary<string, List<EntityValues>>();

            //Get intentions on file
            var csv = new Chilkat.Csv
            {
                //  Prior to loading the CSV file, indicate that the 1st row
                //  should be treated as column names:
                HasColumnNames = true,

            };

            //  Load the CSV records from the entites file:
            var success = csv.LoadFile2(EntitiesFilePath.Value, "utf-8");
            if (!success)
            {
                Console.WriteLine(csv.LastErrorText);
                return;
            }

            //Get entities on file
            //  Display the contents of the 3rd column (i.e. the country names)
            for (int row = 0; row <= csv.NumRows - 1; row++)
            {
                var entityName = csv.GetCell(row, 0);
                var value = csv.GetCell(row, 1);
                var synonymous = csv.GetCell(row, 2);
                var synonymousList = synonymous.Split('/').Where(s => s.Length > 0).ToArray();

                var entitiesValuesList = entitiesMap.ContainsKey(entityName) ? entitiesMap[entityName] : new List<EntityValues>();

                var entity = new EntityValues
                {
                    Name = value,
                    Synonymous = synonymousList.Length == 0 ? null : synonymousList.ToArray()
                };

                entitiesValuesList.Add(entity);
                entitiesMap[entityName] = entitiesValuesList;
            }

            //Add each intention on BLiP IA model
            foreach (var entityKey in entitiesMap.Keys)
            {
                var entity = new Entity
                {
                    Name = entityKey,
                    Values = entitiesMap[entityKey].ToArray()
                };

                await _blipAIClient.AddEntity(entity);
            }
        }
    }
}
