using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface IInternalLogger : ILoggerEnabler, ILogger
    {
    }
}
