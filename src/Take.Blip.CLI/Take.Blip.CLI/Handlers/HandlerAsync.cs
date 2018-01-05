using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Take.BlipCLI.Handlers
{
    public abstract class HandlerAsync
    {
        public int Run(string[] args)
        {
            return RunAsync(args).GetAwaiter().GetResult();
        }

        public abstract Task<int> RunAsync(string[] args);
    }
}
