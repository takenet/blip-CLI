using ITGlobal.CommandLine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Take.BlipCLI.Handlers
{
    public abstract class HandlerAsync
    {
        public ISwitch Force { get; set; }
        public ISwitch Verbose { get; set; }

        public int Run(string[] args)
        {
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
