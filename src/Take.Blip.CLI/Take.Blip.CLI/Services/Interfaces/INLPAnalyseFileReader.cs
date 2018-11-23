using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Models.NLPAnalyse;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface INLPAnalyseFileReader
    {
        Task<List<InputWithTags>> GetInputsToAnalyseAsync(string pathToFile);
        
    }
}
