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
        private readonly IFileManagerService _fileManagerService;
        private readonly ILogger _logger;

        public ExportService(
            IBlipClientFactory blipClientFactory, 
            IExcelGeneratorService excelGeneratorService,
            IFileManagerService fileManagerService,
            ILogger logger)
        {
            _blipClientFactory = blipClientFactory;
            _excelGeneratorService = excelGeneratorService;
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

            var intentions = await blipAIClient.GetAllIntents();
            var entities = await blipAIClient.GetAllEntities();

            _fileManagerService.CreateDirectoryIfNotExists(outputFilePath);

            if (!string.IsNullOrEmpty(excel))
            {
                List<NLPExcelExportModel> excelExportModels = new List<NLPExcelExportModel>
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
                WriteIntentionCSV(intentions, outputFilePath);
                WriteAnswersCSV(intentions, outputFilePath);
                WriteEntitiesCSV(entities, outputFilePath);
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

        private NLPExcelExportModel WriteQuestionsExcel(List<Intention> intentions)
        {
            int RowCount = 0;
            int TotalQuestions = 0;

            NLPExcelExportModel excelExportModel = new NLPExcelExportModel();
            excelExportModel.SheetName = "Questions";

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

            excelExportModel.SheetValues = new string[TotalQuestions, excelExportModel.Columns.Length];

            foreach (var intent in filteredIntentions)
            {
                if (intent.Questions == null)
                {
                    excelExportModel.SheetValues[RowCount, 0] = intent.Name;
                    excelExportModel.SheetValues[RowCount, 1] = string.Empty;

                    RowCount++;
                    continue;
                }

                foreach (var question in intent.Questions)
                {
                    excelExportModel.SheetValues[RowCount, 0] = intent.Name;
                    excelExportModel.SheetValues[RowCount, 1] = question.Text;

                    RowCount++;
                }
            }

            return excelExportModel;
        }

        private NLPExcelExportModel WriteEntitiesExcel(List<Entity> entities)
        {
            int RowCount = 0;
            int TotalEntitiesValue = 0;

            NLPExcelExportModel excelExportModel = new NLPExcelExportModel();
            excelExportModel.SheetName = "Entities";

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

            excelExportModel.SheetValues = new string[TotalEntitiesValue, excelExportModel.Columns.Length];

            foreach (var entity in filteredEntities)
            {
                if (entity.Values == null)
                {
                    excelExportModel.SheetValues[RowCount, 0] = entity.Id;
                    excelExportModel.SheetValues[RowCount, 1] = entity.Name;
                    excelExportModel.SheetValues[RowCount, 2] = string.Empty;
                    excelExportModel.SheetValues[RowCount, 3] = string.Empty;

                    RowCount++;
                    continue;
                }

                foreach (var item in entity.Values)
                {
                    excelExportModel.SheetValues[RowCount, 0] = entity.Id;
                    excelExportModel.SheetValues[RowCount, 1] = entity.Name;
                    excelExportModel.SheetValues[RowCount, 2] = item.Name;
                    excelExportModel.SheetValues[RowCount, 3] = string.Join(";", item.Synonymous);

                    RowCount++;
                }
            }

            return excelExportModel;
        }

        private NLPExcelExportModel WriteAnswersExcel(List<Intention> intentions)
        {
            int RowCount = 0;
            int TotalAnswers = 0;

            NLPExcelExportModel excelExportModel = new NLPExcelExportModel();
            excelExportModel.SheetName = "Answers";

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

            excelExportModel.SheetValues = new string[TotalAnswers, excelExportModel.Columns.Length];

            foreach (var intent in filteredIntentions)
            {
                if (intent.Answers == null)
                {
                    excelExportModel.SheetValues[RowCount, 0] = intent.Name;
                    excelExportModel.SheetValues[RowCount, 1] = string.Empty;

                    RowCount++;
                    continue;
                }

                foreach (var answer in intent.Answers)
                {
                    excelExportModel.SheetValues[RowCount, 0] = intent.Name;
                    excelExportModel.SheetValues[RowCount, 1] = answer.Value.ToString();

                    RowCount++;
                }
            }

            return excelExportModel;
        }

        private NLPExcelExportModel WriteIntentionExcel(List<Intention> intentions)
        {
            int RowCount = 0;
            NLPExcelExportModel excelExportModel = new NLPExcelExportModel();
            excelExportModel.SheetName = "Intentions";

            excelExportModel.Columns = new string[3];

            excelExportModel.Columns[0] = "ID";
            excelExportModel.Columns[1] = "Intention Name";
            excelExportModel.Columns[2] = "Updated At";

            List<Intention> filteredIntentions = intentions.Where(x => x.IsDeleted == false).ToList();

            excelExportModel.SheetValues = new string[filteredIntentions.Count, excelExportModel.Columns.Length];

            foreach (var intent in filteredIntentions)
            {
                excelExportModel.SheetValues[RowCount, 0] = intent.Id;
                excelExportModel.SheetValues[RowCount, 1] = intent.Name;
                excelExportModel.SheetValues[RowCount, 2] = intent.StorageDate.GetValueOrDefault().ToString("dd/MM/yyyy hh:mm:ss");

                RowCount++;
            }

            return excelExportModel;
        }

        #endregion

        #region CSV Generation
        private void WriteEntitiesCSV(List<Entity> entities, string outputPath)
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

            csv.SaveFile(Path.Combine(outputPath, "entities.csv"));
        }

        private void WriteAnswersCSV(List<Intention> intentions, string outputPath)
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

            csv.SaveFile(Path.Combine(outputPath, "answers.csv"));
        }

        private void WriteIntentionCSV(List<Intention> intentions, string outputPath)
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

            var path = Path.Combine(outputPath, "intentions.csv");
            csv.SaveFile(path);
        }

        
        #endregion
    }
}
