using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Models;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface INLPAnalyseFileWriter
    {
        Task WriteAnalyseReportAsync(NLPAnalyseReport analyseReport);
    }
}
