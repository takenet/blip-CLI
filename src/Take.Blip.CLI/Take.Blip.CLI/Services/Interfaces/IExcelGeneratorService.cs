using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Models;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface IExcelGeneratorService
    {
        void WriteContentOnExcel(List<NLPExcelExportModel> excelExportModel, string directory, string fileName);
    }
}
