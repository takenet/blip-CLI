using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Models;
using Take.BlipCLI.Models.NLPAnalyse;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface INLPAnalyseFileWriter
    {
        Task WriteAnalyseReportAsync(Report analyseReport, bool append = false);
    }
}
