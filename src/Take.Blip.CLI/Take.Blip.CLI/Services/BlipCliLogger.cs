using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Services
{
    public class BlipCliLogger : IInternalLogger
    {
        private static ILogger _logger = null;

        private readonly ILoggerProviderFactory _loggerProviderFactory;
        private LogLevel _mininumLogLevel;
        

        public BlipCliLogger(ILoggerProviderFactory loggerProviderFactory)
        {
            _mininumLogLevel = LogLevel.Warning;
            _loggerProviderFactory = loggerProviderFactory;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return GetInstance().BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return GetInstance().IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            GetInstance().Log(logLevel, eventId, state, exception, formatter);
        }

        public void SetLogLevelByVerbosity(bool verbose)
        {
            _mininumLogLevel = verbose ? LogLevel.Trace : LogLevel.Warning;
        }
        private ILogger GetInstance()
        {
            if(_logger == null)
            {
                _logger = _loggerProviderFactory.GetLoggerByVerbosity(IsVerbose());
            }
            return _logger;
        }
        
        private bool IsVerbose()
        {
            return _mininumLogLevel == LogLevel.Trace;
        }

        
    }
}
