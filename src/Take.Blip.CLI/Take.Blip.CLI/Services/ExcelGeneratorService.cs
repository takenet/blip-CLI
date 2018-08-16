using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Take.BlipCLI.Models;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Services
{
    public class ExcelGeneratorService : IExcelGeneratorService
    {
        public void WriteContentOnExcel(List<NLPExcelExportModel> excelExportModel, string directory, string fileName)
        {
            using (var package = new ExcelPackage(CreateExcelFileInfo(directory, fileName)))
            {
                foreach (var item in excelExportModel)
                {
                    int RowCount = 2;
                    ExcelWorksheet worksheet = CreateExcelWorkSheet(package, item.SheetName);

                    //Write Columns 
                    for (int column = 0; column < item.Columns.Length; column++)
                        worksheet.Cells[1, column + 1].Value = item.Columns[column];

                    //Format Title Cells with different color
                    using (var range = worksheet.Cells[1, 1, 1, item.Columns.Length])
                        FormatTitleCells(range);

                    //Write Values on sheet
                    for (int rows = 0; rows < item.SheetValues.GetLength(0); rows++)
                    {
                        for (int columns = 0; columns < item.SheetValues.GetLength(1); columns++)
                        {
                            worksheet.Cells[RowCount, columns + 1].Value = item.SheetValues[rows, columns];
                        }
                        RowCount++;
                    }

                    //Set Columns Width
                    for (int columns = 0; columns < item.Columns.Length; columns++)
                        SetColumnWidthFit(worksheet, columns + 1);
                }

                package.Save();
            }

        }

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

        private void SetColumnWidthFit(ExcelWorksheet worksheet, int columnIndex) => worksheet.Column(columnIndex).AutoFit();

        private void FormatTitleCells(ExcelRange excelRange)
        {
            excelRange.Style.Font.Bold = true;
            excelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            excelRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);

        }
    }
}
