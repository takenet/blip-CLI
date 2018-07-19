using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Handlers
{
    public class NLPExportHandler : ExportHandler
    {
        private IBlipAIClient _blipAIClient;

        public NLPExportHandler(IBlipClientFactory blipClientFactory) : base(blipClientFactory)
        {

        }

        public static NLPExportHandler GetInstance(ExportHandler eh)
        {
            return new NLPExportHandler (eh.BlipClientFactory)
            {
                Node = eh.Node,
                Authorization = eh.Authorization,
                OutputFilePath = eh.OutputFilePath,
                Model = eh.Model,
                Verbose = eh.Verbose
            };
        }

        public override async Task<int> RunAsync(string[] args)
        {
            string authorization = GetAuthorization();

            _blipAIClient = new BlipHttpClientAsync(authorization);

            LogVerboseLine("NLP Export");

            var intentions = await _blipAIClient.GetAllIntents(verbose: Verbose.IsSet);

            var entities = await _blipAIClient.GetAllEntities(verbose: Verbose.IsSet);

            Directory.CreateDirectory(OutputFilePath.Value);

            WriteIntention(intentions);

            WriteAnswers(intentions);

            WriteEntities(entities);

            LogVerboseLine("DONE");

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
