using ITGlobal.CommandLine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Services;

namespace Take.BlipCLI.Handlers
{
    public class NLPAnalyseHandler : HandlerAsync
    {
        public INamedParameter<string> Text { get; set; }
        public INamedParameter<string> Node { get; set; }
        public INamedParameter<string> AccessKey { get; set; }

        public override async Task<int> RunAsync(string[] args)
        {
            var client = new BlipHttpClientAsync(AccessKey.Value);
            var result = await client.AnalyseForMetrics(Text.Value);

            using (CLI.WithForeground(ConsoleColor.Red))
            {
                Console.WriteLine("This text will be written in RED font on default background");

                using (CLI.WithForeground(ConsoleColor.White))
                {
                    using (CLI.WithBackground(ConsoleColor.Red))
                    {
                        Console.WriteLine("This text will be written in WHITE font on RED background");
                    }

                    Console.WriteLine("And this text will be written in WHITE font on default background");
                }
            }

            return 0;
        }
    }   
}
