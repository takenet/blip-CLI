using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Models.NLPAnalyse;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface IJitterService
    {
        Task<string> ApplyJitterAsync(string source, JitterType jitterType, double probabilityScaleFactor = 0.5, int maxWordSize = 15);
        Task<string> ApplyJitterAsync(string source, JitterDistribution jitterDistribution, double probabilityScaleFactor = 0.5, int maxWordSize = 15);
    }
}
