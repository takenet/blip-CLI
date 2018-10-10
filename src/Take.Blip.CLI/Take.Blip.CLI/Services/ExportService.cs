using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Models;
using Take.BlipCLI.Services.Interfaces;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Services
{
    public class ExportService : IExportService
    {
        private readonly IBlipClientFactory _blipClientFactory;
        private readonly IExcelGeneratorService _excelGeneratorService;
        private readonly ICSVGeneratorService _csvGeneratorService;
        private readonly IFileManagerService _fileManagerService;
        private readonly IInternalLogger _logger;

        public ExportService(
            IBlipClientFactory blipClientFactory,
            IExcelGeneratorService excelGeneratorService,
            ICSVGeneratorService csvGeneratorService,
            IFileManagerService fileManagerService,
            IInternalLogger logger)
        {
            _blipClientFactory = blipClientFactory;
            _excelGeneratorService = excelGeneratorService;
            _csvGeneratorService = csvGeneratorService;
            _fileManagerService = fileManagerService;
            _logger = logger;
        }


        public async Task ExportNLPModelAsync(string authorization, string outputFilePath, string excel = null)
        {
            if (string.IsNullOrEmpty(authorization))
                throw new ArgumentNullException(nameof(authorization));

            if (string.IsNullOrEmpty(outputFilePath))
                throw new ArgumentNullException(nameof(outputFilePath));

            var blipAIClient = _blipClientFactory.GetInstanceForAI(authorization);

            _logger.LogDebug("NLP Export\n");

            var intentions = await blipAIClient.GetAllIntentsAsync();
            var entities = await blipAIClient.GetAllEntities();

            _fileManagerService.CreateDirectoryIfNotExists(outputFilePath);

            if (!string.IsNullOrEmpty(excel))
            {
                List<NLPExportModel> excelExportModels = new List<NLPExportModel>
                {
                    WriteIntentionExcel(intentions),
                    WriteQuestionsExcel(intentions),
                    WriteAnswersExcel(intentions),
                    WriteEntitiesExcel(entities)
                };

                _excelGeneratorService.WriteContentOnExcel(excelExportModels, outputFilePath, excel);
            }
            else
            {
                var csvExportModels = new List<NLPExportModel>
                {
                    WriteIntentionCSV(intentions, outputFilePath),
                    WriteAnswersCSV(intentions, outputFilePath),
                    WriteEntitiesCSV(entities, outputFilePath)

                };
                _csvGeneratorService.WriteContentOnCSV(csvExportModels, outputFilePath);

            }

            _logger.LogDebug("DONE");

        }

        public async Task ExportContentByKeyAsync(string authorization, string key, string outputFilePath, string excel = null)
        {
            if (string.IsNullOrEmpty(authorization))
                throw new ArgumentNullException(nameof(authorization));

            if (string.IsNullOrEmpty(outputFilePath))
                throw new ArgumentNullException(nameof(outputFilePath));

            var blipClient = _blipClientFactory.GetInstanceForBucket(authorization);

            //var key = "blip_portal:builder_working_flow";

            _logger.LogDebug("Bucket Export\n");

            var data = await blipClient.GetDocumentAsync(key, BucketNamespace.Document);

            if (data.HasValue)
            {
                var asString = JsonConvert.SerializeObject(data.Value.Value);
                var flow = Path.Combine(outputFilePath, "bucket.json");
                _fileManagerService.CreateDirectoryIfNotExists(outputFilePath);
                using (var fw = new StreamWriter(flow, false, Encoding.UTF8))
                {
                    await fw.WriteAsync(asString);
                }
            }

            _logger.LogDebug("DONE\n");
        }

        #region Excel Generation

        private NLPExportModel WriteQuestionsExcel(List<Intention> intentions)
        {
            int RowCount = 0;
            int TotalQuestions = 0;

            NLPExportModel excelExportModel = new NLPExportModel();
            excelExportModel.Name = "Questions";

            excelExportModel.Columns = new string[2];

            excelExportModel.Columns[0] = "Intention Name";
            excelExportModel.Columns[1] = "Question";

            List<Intention> filteredIntentions = intentions.Where(x => x.IsDeleted == false).ToList();

            filteredIntentions.ForEach(delegate (Intention intent)
            {
                Question[] questions = intent.Questions;

                if (questions == null)
                    TotalQuestions = TotalQuestions + 1;
                else
                    TotalQuestions = TotalQuestions + questions.Length;
            });

            excelExportModel.Values = new string[TotalQuestions, excelExportModel.Columns.Length];

            foreach (var intent in filteredIntentions)
            {
                if (intent.Questions == null)
                {
                    excelExportModel.Values[RowCount, 0] = intent.Name;
                    excelExportModel.Values[RowCount, 1] = string.Empty;

                    RowCount++;
                    continue;
                }

                foreach (var question in intent.Questions)
                {
                    excelExportModel.Values[RowCount, 0] = intent.Name;
                    excelExportModel.Values[RowCount, 1] = question.Text;

                    RowCount++;
                }
            }

            return excelExportModel;
        }

        private NLPExportModel WriteEntitiesExcel(List<Entity> entities)
        {
            int RowCount = 0;
            int TotalEntitiesValue = 0;

            NLPExportModel excelExportModel = new NLPExportModel();
            excelExportModel.Name = "Entities";

            excelExportModel.Columns = new string[4];

            excelExportModel.Columns[0] = "ID";
            excelExportModel.Columns[1] = "Entity Name";
            excelExportModel.Columns[2] = "Value Name";
            excelExportModel.Columns[3] = "Synonymous";

            List<Entity> filteredEntities = entities.Where(x => x.IsDeleted == false).ToList();

            filteredEntities.ForEach(delegate (Entity entity)
            {
                EntityValues[] questions = entity.Values;

                if (questions == null)
                    TotalEntitiesValue = TotalEntitiesValue + 1;
                else
                    TotalEntitiesValue = TotalEntitiesValue + questions.Length;
            });

            excelExportModel.Values = new string[TotalEntitiesValue, excelExportModel.Columns.Length];

            foreach (var entity in filteredEntities)
            {
                if (entity.Values == null)
                {
                    excelExportModel.Values[RowCount, 0] = entity.Id;
                    excelExportModel.Values[RowCount, 1] = entity.Name;
                    excelExportModel.Values[RowCount, 2] = string.Empty;
                    excelExportModel.Values[RowCount, 3] = string.Empty;

                    RowCount++;
                    continue;
                }

                foreach (var item in entity.Values)
                {
                    excelExportModel.Values[RowCount, 0] = entity.Id;
                    excelExportModel.Values[RowCount, 1] = entity.Name;
                    excelExportModel.Values[RowCount, 2] = item.Name;
                    excelExportModel.Values[RowCount, 3] = string.Join(";", item.Synonymous);

                    RowCount++;
                }
            }

            return excelExportModel;
        }

        private NLPExportModel WriteAnswersExcel(List<Intention> intentions)
        {
            int RowCount = 0;
            int TotalAnswers = 0;

            NLPExportModel excelExportModel = new NLPExportModel();
            excelExportModel.Name = "Answers";

            excelExportModel.Columns = new string[2];

            excelExportModel.Columns[0] = "Intention Name";
            excelExportModel.Columns[1] = "Answer";

            List<Intention> filteredIntentions = intentions.Where(x => x.IsDeleted == false).ToList();

            filteredIntentions.ForEach(delegate (Intention intent)
            {
                Answer[] questions = intent.Answers;

                if (questions == null)
                    TotalAnswers = TotalAnswers + 1;
                else
                    TotalAnswers = TotalAnswers + questions.Length;
            });

            excelExportModel.Values = new string[TotalAnswers, excelExportModel.Columns.Length];

            foreach (var intent in filteredIntentions)
            {
                if (intent.Answers == null)
                {
                    excelExportModel.Values[RowCount, 0] = intent.Name;
                    excelExportModel.Values[RowCount, 1] = string.Empty;

                    RowCount++;
                    continue;
                }

                foreach (var answer in intent.Answers)
                {
                    excelExportModel.Values[RowCount, 0] = intent.Name;
                    excelExportModel.Values[RowCount, 1] = answer.Value.ToString();

                    RowCount++;
                }
            }

            return excelExportModel;
        }

        private NLPExportModel WriteIntentionExcel(List<Intention> intentions)
        {
            int RowCount = 0;
            NLPExportModel excelExportModel = new NLPExportModel();
            excelExportModel.Name = "Intentions";

            excelExportModel.Columns = new string[3];

            excelExportModel.Columns[0] = "ID";
            excelExportModel.Columns[1] = "Intention Name";
            excelExportModel.Columns[2] = "Updated At";

            List<Intention> filteredIntentions = intentions.Where(x => x.IsDeleted == false).ToList();

            excelExportModel.Values = new string[filteredIntentions.Count, excelExportModel.Columns.Length];

            foreach (var intent in filteredIntentions)
            {
                excelExportModel.Values[RowCount, 0] = intent.Id;
                excelExportModel.Values[RowCount, 1] = intent.Name;
                excelExportModel.Values[RowCount, 2] = intent.StorageDate.GetValueOrDefault().ToString("dd/MM/yyyy hh:mm:ss");

                RowCount++;
            }

            return excelExportModel;
        }

        #endregion

        #region CSV Generation
        private NLPExportModel WriteEntitiesCSV(List<Entity> entities, string outputPath)
        {
            if (entities == null)
                return null;

            var total = entities.Sum((entity) => entity.Values == null ? 0 : entity.Values.Length);

            var model = new NLPExportModel
            {
                Columns = new string[] { "Entity", "Value", "Synonymous" },
                Values = new string[total, 3]
            };

            var i = 0;
            foreach (var entity in entities)
            {
                if (entity.Values == null) continue;
                foreach (var value in entity.Values)
                {
                    model.Values[i, 0] = entity.Name;
                    model.Values[i, 1] = value.Name;
                    model.Values[i, 2] = string.Join('/', value.Synonymous);
                    i++;
                }
            }
            model.Name = "entities.csv";

            return model;
        }

        private NLPExportModel WriteAnswersCSV(List<Intention> intentions, string outputPath)
        {
            if (intentions == null)
                return null;

            var total = intentions.Sum((intention) => intention.Answers == null ? 0 : intention.Answers.Length);

            var model = new NLPExportModel
            {
                Columns = new string[] { "Intent", "Answer" },
                Values = new string[total, 2]
            };

            var i = 0;
            foreach (var intent in intentions)
            {
                if (intent.Answers == null) continue;
                foreach (var answers in intent.Answers)
                {
                    model.Values[i, 0] = intent.Name;
                    model.Values[i, 1] = answers.Value.ToString();
                    i++;
                }
            }
            model.Name = "answers.csv";

            return model;
        }

        private NLPExportModel WriteIntentionCSV(List<Intention> intentions, string outputPath)
        {
            if (intentions == null)
                return null;

            var total = intentions.Sum((intention) => intention.Questions == null ? 0 : intention.Questions.Length);

            var model = new NLPExportModel
            {
                Columns = new string[] { "Intent", "Question" },
                Values = new string[total, 2]
            };

            var i = 0;
            foreach (var intent in intentions)
            {
                if (intent.Questions == null) continue;
                foreach (var question in intent.Questions)
                {
                    model.Values[i, 0] = intent.Name;
                    model.Values[i, 1] = question.Text;
                    i++;
                }
            }

            model.Name = "intentions.csv";

            return model;
        }


        #endregion
    }
}
