using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface INLPModelExportService
    {
        Task ExportNLPModelAsync(string authorization, string outputFilePath, string excel = null);
    }
}
