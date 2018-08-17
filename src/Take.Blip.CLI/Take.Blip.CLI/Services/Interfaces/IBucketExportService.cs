using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface IBucketExportService
    {
        Task ExportContentByKeyAsync(string authorization, string key, string outputFilePath, string excel = null);
    }
}
