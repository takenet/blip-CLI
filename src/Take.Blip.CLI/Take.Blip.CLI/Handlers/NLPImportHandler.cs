using ITGlobal.CommandLine;
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

        private readonly ISettingsFile _settingsFile;
        private IBlipAIClient _blipAIClient;

        public NLPImportHandler()
        {
            _settingsFile = new SettingsFile();
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

            _blipAIClient = new BlipHttpClientAsync(authorization);

            if (IntentsFilePath.IsSet)
            {
                Console.WriteLine("Starting intents import task...");
                await ImportIntentions();
                Console.WriteLine("Intents imported with success...");
            }

            if (EntitiesFilePath.IsSet)
            {
                Console.WriteLine("Starting entities import task...");
                await ImportEntities();
                Console.WriteLine("Entities imported with success...");
            }

            return 0;
        }

        private async Task ImportIntentions()
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
                return;
            }

            //  Display the contents of the 3rd column (i.e. the country names)
            for (int row = 0; row <= csv.NumRows - 1; row++)
            {
                var question = csv.GetCell(row, 0);
                var intentionName = csv.GetCell(row, 1);

                var questionsList = intentionsMap.ContainsKey(intentionName) ? intentionsMap[intentionName] : new List<string>();

                questionsList.Add(question);
                intentionsMap[intentionName] = questionsList;
            }

            //Add each intention on BLiP IA model
            foreach (var intentionKey in intentionsMap.Keys)
            {
                var id = await _blipAIClient.AddIntent(intentionKey);

                var questionsList = intentionsMap[intentionKey];
                var questionsArray = questionsList.Select(q => new Question { Text = q }).ToArray();

                await _blipAIClient.AddQuestions(id, questionsArray);
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
                HasColumnNames = true
            };

            //  Load the CSV records from the entites file:
            var success = csv.LoadFile(EntitiesFilePath.Value);
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
                var synonymousList = synonymous.Split(';');

                var entitiesValuesList = entitiesMap.ContainsKey(entityName) ? entitiesMap[entityName] : new List<EntityValues>();

                var entity = new EntityValues
                {
                    Name = value,
                    Synonymous = synonymousList.ToArray()
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
