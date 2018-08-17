using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Models;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface ICSVGeneratorService
    {
        void WriteContentOnCSV(List<NLPExportModel> models, string outputPath);
    }
}
