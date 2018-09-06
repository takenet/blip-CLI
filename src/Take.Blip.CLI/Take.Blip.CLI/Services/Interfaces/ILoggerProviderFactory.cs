using Microsoft.Extensions.Logging;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface ILoggerProviderFactory
    {
        ILogger GetLoggerByVerbosity(bool verbose);
    }
}