using System.Threading.Tasks;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface INLPAnalyseService
    {
        Task AnalyseAsync(string authorization, string inputSource, string reportOutput, bool doContentCheck = false);
    }
}