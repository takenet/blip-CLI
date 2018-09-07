using ITGlobal.CommandLine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Handlers
{
    public abstract class HandlerAsync
    {   
        public ISwitch Force { get; set; }
        public ISwitch Verbose { get; set; }
        public ISwitch VeryVerbose { get; set; }

        protected readonly IInternalLogger _logger;

        public HandlerAsync(IInternalLogger internalLogger)
        {
            _logger = internalLogger;
        }

        public int Run(string[] args)
        {
            _logger.SetLogLevelByVerbosity(Verbose.IsSet, VeryVerbose.IsSet);
            return RunAsync(args).GetAwaiter().GetResult();
        }

        public abstract Task<int> RunAsync(string[] args);

        public string GetTypesListAsString<T>()
        {
            var validContents = Enum.GetNames(typeof(T));
            return string.Join(", ", validContents);
        }

        protected void LogVerbose(string message)
        {
            if (Verbose.IsSet) Console.Write(message);
        }

        protected void LogVerboseLine(string message)
        {
            if (Verbose.IsSet) Console.WriteLine(message);
        }

    }
}
