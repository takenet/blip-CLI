using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Services
{
    public class LoggerProviderFactory : ILoggerProviderFactory
    {
        public ILogger GetLoggerByVerbosity(bool verbose, bool veryVerbose)
        {
            if (veryVerbose)
            {
                return new ConsoleLogger("blip-CLI",
                    (input, logLevel) =>
                    {
                        return true;
                    }, false);
            }
            else if (verbose)
            {
                return new ConsoleLogger("blip-CLI",
                    (input, logLevel) =>
                    {
                        if (logLevel >= LogLevel.Debug)
                            return true;
                        return false;
                    }, false);
            }
            else
            {
                return new ConsoleLogger("blip-CLI",
                    (input, logLevel) =>
                    {
                        if (logLevel >= LogLevel.Warning)
                            return true;
                        return false;
                    }, false);
            }
        }
    }
}
