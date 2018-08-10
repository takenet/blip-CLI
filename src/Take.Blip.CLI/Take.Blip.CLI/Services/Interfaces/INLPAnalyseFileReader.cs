using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface INLPAnalyseFileReader
    {
        Task<List<string>> GetInputsToAnalyseAsync(string pathToFile);
        
    }
}
