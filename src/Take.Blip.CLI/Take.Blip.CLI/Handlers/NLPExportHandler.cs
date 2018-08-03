using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

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
            return new NLPExportHandler(eh.BlipClientFactory)
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

                using (var package = new ExcelPackage(CreateExcelFileInfo(OutputFilePath.Value, Excel.Value)))
                {
                    WriteIntentionExcel(intentions, package);

                    WriteQuestionsExcel(intentions, package);

                    WriteAnswersExcel(intentions, package);

                    WriteEntitiesExcel(entities, package);

                    package.Save();
                }

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

        private FileInfo CreateExcelFileInfo(string directory, string fileName)
        {
            string fileFullPath = Path.Combine(directory, $"{fileName}.xlsx");

            var newFile = new FileInfo(fileFullPath);

            if (newFile.Exists)
            {
                newFile.Delete();
                newFile = new FileInfo(fileFullPath);
            }
            return newFile;
        }
        private ExcelWorksheet CreateExcelWorkSheet(ExcelPackage excelPackage, string worksheetName) => excelPackage.Workbook.Worksheets.Add(worksheetName);
        private void FormatTitleCells(ExcelRange excelRange)
        {
            excelRange.Style.Font.Bold = true;
            excelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            excelRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);

        }
        private void SetColumnWidthFit(ExcelWorksheet worksheet, int columnIndex) => worksheet.Column(columnIndex).AutoFit();

        private void WriteQuestionsExcel(List<Intention> intentions, ExcelPackage excelPackage)
        {
            int RowCount = 2;

            ExcelWorksheet worksheet = CreateExcelWorkSheet(excelPackage, "Questions");

            worksheet.Cells[1, 1].Value = "Intention Name";
            worksheet.Cells[1, 2].Value = "Question";

            using (var range = worksheet.Cells[1, 1, 1, 2])
            {
                FormatTitleCells(range);
            }

            foreach (var intent in intentions)
            {
                if (intent.Questions == null)
                {
                    worksheet.Cells[RowCount, 1].Value = intent.Name;
                    worksheet.Cells[RowCount, 2].Value = string.Empty;

                    RowCount++;

                    continue;
                }

                foreach (var question in intent.Questions)
                {
                    worksheet.Cells[RowCount, 1].Value = intent.Name;
                    worksheet.Cells[RowCount, 2].Value = question.Text;

                    RowCount++;
                }
            }

            SetColumnWidthFit(worksheet, 1);
            SetColumnWidthFit(worksheet, 2);
        }


        private void WriteEntitiesExcel(List<Entity> entities, ExcelPackage excelPackage)
        {
            int RowCount = 2;

            ExcelWorksheet worksheet = CreateExcelWorkSheet(excelPackage, "Entities");

            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "Entity Name";
            worksheet.Cells[1, 3].Value = "Value Name";
            worksheet.Cells[1, 4].Value = "Synonymous";

            using (var range = worksheet.Cells[1, 1, 1, 4])
            {
                FormatTitleCells(range);
            }

            foreach (var entity in entities)
            {
                if (entity.Values == null)
                {
                    worksheet.Cells[RowCount, 1].Value = entity.Id;
                    worksheet.Cells[RowCount, 2].Value = entity.Name;
                    worksheet.Cells[RowCount, 3].Value = string.Empty;
                    worksheet.Cells[RowCount, 4].Value = string.Empty;

                    RowCount++;

                    continue;
                }

                foreach (var item in entity.Values)
                {
                    worksheet.Cells[RowCount, 1].Value = entity.Id;
                    worksheet.Cells[RowCount, 2].Value = entity.Name;
                    worksheet.Cells[RowCount, 3].Value = item.Name;
                    worksheet.Cells[RowCount, 4].Value = string.Join(";", item.Synonymous);

                    RowCount++;
                }
            }

            SetColumnWidthFit(worksheet, 1);
            SetColumnWidthFit(worksheet, 2);
            SetColumnWidthFit(worksheet, 3);
            SetColumnWidthFit(worksheet, 4);
        }

        private void WriteAnswersExcel(List<Intention> intentions, ExcelPackage excelPackage)
        {
            int RowCount = 2;

            ExcelWorksheet worksheet = CreateExcelWorkSheet(excelPackage, "Answers");
            worksheet.Cells[1, 1].Value = "Intention";
            worksheet.Cells[1, 2].Value = "Answer";

            using (var range = worksheet.Cells[1, 1, 1, 2])
            {
                FormatTitleCells(range);
            }

            foreach (var intent in intentions)
            {
                if (intent.Answers == null)
                {
                    worksheet.Cells[RowCount, 1].Value = intent.Name;
                    worksheet.Cells[RowCount, 2].Value = string.Empty;

                    RowCount++;

                    continue;
                }

                foreach (var answer in intent.Answers)
                {
                    worksheet.Cells[RowCount, 1].Value = intent.Name;
                    worksheet.Cells[RowCount, 2].Value = answer.Value.ToString();

                    RowCount++;
                }
            }

            SetColumnWidthFit(worksheet, 1);
            SetColumnWidthFit(worksheet, 2);
        }

        private void WriteIntentionExcel(List<Intention> intentions, ExcelPackage excelPackage)
        {
            int RowCount = 2;

            ExcelWorksheet worksheet = CreateExcelWorkSheet(excelPackage, "Intentions");

            worksheet.Cells[1, 1].Value = "ID";
            worksheet.Cells[1, 2].Value = "Intention Name";
            worksheet.Cells[1, 3].Value = "Updated At";
            worksheet.Cells[1, 4].Value = "Is Deleted";

            using (var range = worksheet.Cells[1, 1, 1, 4])
            {
                FormatTitleCells(range);
            }

            foreach (var item in intentions)
            {
                worksheet.Cells[RowCount, 1].Value = item.Id;
                worksheet.Cells[RowCount, 2].Value = item.Name;
                worksheet.Cells[RowCount, 3].Value = item.StorageDate.GetValueOrDefault().ToString("dd/MM/yyyy hh:mm:ss");
                worksheet.Cells[RowCount, 4].Value = item.IsDeleted == false ? "Não" : "Sim";

                RowCount++;
            }

            SetColumnWidthFit(worksheet, 1);
            SetColumnWidthFit(worksheet, 2);
            SetColumnWidthFit(worksheet, 3);
            SetColumnWidthFit(worksheet, 4);
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
