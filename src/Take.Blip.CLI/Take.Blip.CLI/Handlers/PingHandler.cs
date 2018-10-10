using ITGlobal.CommandLine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Handlers
{
    public class PingHandler : HandlerAsync
    {
        private string VALID_AUTHORIZATION = "dGVzdGVodHRwcG9zdDpzQ3Q4RkEwT3ZMQ1J0UVlHaGd4SA==";
        public INamedParameter<string> Node { get; set; }

        public PingHandler(IInternalLogger logger) : base(logger)
        {

        }

        public override async Task<int> RunAsync(string[] args)
        {
            bool success;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var client = new BlipHttpClientAsync(VALID_AUTHORIZATION);

            using (var spinner = CLI.Spinner("Sending..."))
            {
                success = await client.PingAsync(Node.Value);
            }

            watch.Stop();
            var elapsedMilli = watch.ElapsedMilliseconds;

            if (success)
            {
                Console.WriteLine($"Response from [{Node.Value}]: time={elapsedMilli}ms");
            }
            else
            {
                using (CLI.WithForeground(ConsoleColor.Red))
                {
                    Console.WriteLine($"Without response from [{Node.Value}]: time={elapsedMilli}ms");
                }
            }

            return 0;
        }
    }
}
