using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface ILoggerEnabler
    {
        void SetLogLevelByVerbosity(bool verbose);
    }
}
