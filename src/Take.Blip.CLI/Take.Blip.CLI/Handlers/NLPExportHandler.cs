using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Take.BlipCLI.Models;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Handlers
{
    public class NLPExportHandler : ExportHandler
    {
        private IBlipAIClient _blipAIClient;

        public NLPExportHandler(IBlipClientFactory blipClientFactory, IExcelGeneratorService excelGeneratorService) : base(blipClientFactory, excelGeneratorService)
        {
        }

        public static NLPExportHandler GetInstance(ExportHandler eh)
        {
            return new NLPExportHandler(eh.BlipClientFactory, eh.ExcelGeneratorService)
            {
                Node = eh.Node,
                Authorization = eh.Authorization,
                OutputFilePath = eh.OutputFilePath,
                Model = eh.Model,
                Verbose = eh.Verbose,
                Excel = eh.Excel
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

            if (Excel.IsSet)
            {
                if (Excel.Value == string.Empty || Excel.Value == null)
                    throw new ArgumentNullException("You must provide a file name to save the file.");

                List<NLPExcelExportModel> excelExportModels = new List<NLPExcelExportModel>();

                excelExportModels.Add(WriteIntentionExcel(intentions));

                excelExportModels.Add(WriteQuestionsExcel(intentions));

                excelExportModels.Add(WriteAnswersExcel(intentions));

                excelExportModels.Add(WriteEntitiesExcel(entities));

                ExcelGeneratorService.WriteContentOnExcel(excelExportModels, OutputFilePath.Value, Excel.Value);
            }
            else
            {
                WriteIntentionCSV(intentions);

                WriteAnswersCSV(intentions);

                WriteEntitiesCSV(entities);
            }

            LogVerboseLine("DONE");

            return 0;
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
        private void WriteEntitiesCSV(List<Takenet.Iris.Messaging.Resources.ArtificialIntelligence.Entity> entities)
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

        private void WriteAnswersCSV(List<Takenet.Iris.Messaging.Resources.ArtificialIntelligence.Intention> intentions)
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

        private void WriteIntentionCSV(List<Takenet.Iris.Messaging.Resources.ArtificialIntelligence.Intention> intentions)
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
        #endregion
    }
}
