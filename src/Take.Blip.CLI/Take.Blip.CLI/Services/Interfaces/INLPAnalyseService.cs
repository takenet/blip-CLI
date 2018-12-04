using System.Threading.Tasks;
using Take.BlipCLI.Models.NLPAnalyse;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface INLPAnalyseService
    {
        Task AnalyseAsync(AnalyseParameters analyseParameters);
    }
}