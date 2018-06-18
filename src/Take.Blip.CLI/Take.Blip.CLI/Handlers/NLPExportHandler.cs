using ITGlobal.CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;
using Take.BlipCLI.Services.Settings;

namespace Take.BlipCLI.Handlers
{
    public class NLPExportHandler : HandlerAsync
    {
        public INamedParameter<string> Node { get; set; }
        public INamedParameter<string> Authorization { get; set; }
        public INamedParameter<string> OutputFilePath { get; set; }

        private readonly ISettingsFile _settingsFile;

        private IBlipAIClient _blipAIClient;

        public NLPExportHandler()
        {
            _settingsFile = new SettingsFile();
        }

        public override async Task<int> RunAsync(string[] args)
        {
            if (!Node.IsSet && !Authorization.IsSet)
                throw new ArgumentNullException("You must provide the target bot (node) for this action. Use '-n' [--node] (or '-a' [--authorization]) parameters");

            if (!OutputFilePath.IsSet)
                throw new ArgumentNullException("You must provide the target output path for this action. Use '-o' [--output] parameter");

            string authorization = Authorization.Value;

            if (Node.IsSet)
            {
                authorization = _settingsFile.GetNodeCredentials(Lime.Protocol.Node.Parse(Node.Value)).Authorization;
            }

            _blipAIClient = new BlipHttpClientAsync(authorization);

            var intentions = await _blipAIClient.GetAllIntents(true);

            var entities = await _blipAIClient.GetAllEntities(true);

            Directory.CreateDirectory(OutputFilePath.Value);

            WriteIntention(intentions);

            WriteAnswers(intentions);

            WriteEntities(entities);

            return 0;
        }

        private void WriteEntities(List<Takenet.Iris.Messaging.Resources.ArtificialIntelligence.Entity> entities)
        {
            var csv = new Chilkat.Csv
            {
                Delimiter = ";"
            };
            
            csv.SetCell(0, 0, "Entity");
            csv.SetCell(0, 1, "Value");
            csv.SetCell(0, 2, "Synonymous");

            var i = 1;
            foreach (var entity in entities)
            {
                if (entity.Values == null) continue;
                foreach (var value in entity.Values)
                {
                    csv.SetCell(i, 0, entity.Name);
                    csv.SetCell(i, 1, value.Name);
                    csv.SetCell(i, 2, string.Join('/', value.Synonymous));
                    i++;
                }
            }

            csv.SaveFile(Path.Combine(OutputFilePath.Value, "entities.csv"));
        }

        private void WriteAnswers(List<Takenet.Iris.Messaging.Resources.ArtificialIntelligence.Intention> intentions)
        {
            var csv = new Chilkat.Csv
            {
                Delimiter = ";",
            };

            csv.SetCell(0, 0, "Intent");
            csv.SetCell(0, 1, "Answer");

            var i = 1;
            foreach (var intent in intentions)
            {
                if (intent.Answers == null) continue;
                foreach (var answers in intent.Answers)
                {
                    csv.SetCell(i, 0, intent.Name);
                    csv.SetCell(i, 1, answers.Value.ToString());
                    i++;
                }
            }

            csv.SaveFile(Path.Combine(OutputFilePath.Value, "answers.csv"));
        }

        private void WriteIntention(List<Takenet.Iris.Messaging.Resources.ArtificialIntelligence.Intention> intentions)
        {
            var csv = new Chilkat.Csv
            {
                Delimiter = ";",
            };
            
            csv.SetCell(0, 0, "Intent");
            csv.SetCell(0, 1, "Question");

            var i = 1;
            foreach (var intent in intentions)
            {
                if (intent.Questions == null) continue;
                foreach (var question in intent.Questions)
                {
                    csv.SetCell(i, 0, intent.Name);
                    csv.SetCell(i, 1, question.Text);
                    i++;
                }
            }
            
            var path = Path.Combine(OutputFilePath.Value, "intentions.csv");
            csv.SaveFile(path);
        }
    }
}
