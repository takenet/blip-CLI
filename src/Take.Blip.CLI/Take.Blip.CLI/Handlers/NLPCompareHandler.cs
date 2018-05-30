using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITGlobal.CommandLine;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Handlers
{
    public class NLPCompareHandler : HandlerAsync
    {
        private readonly IStringService _stringService;

        public INamedParameter<string> Authorization1 { get; internal set; }
        public INamedParameter<string> Authorization2 { get; internal set; }
        public INamedParameter<string> OutputFilePath { get; internal set; }
        public INamedParameter<ComparisonMethod> Method { get; internal set; }
        public ISwitch Verbose { get; internal set; }
        public bool IsVerbose { get => Verbose.IsSet; }

        public NLPCompareHandler(IStringService stringService)
        {
            this._stringService = stringService;
        }

        public override async Task<int> RunAsync(string[] args)
        {
            if (!Authorization1.IsSet || !Authorization2.IsSet)
                throw new ArgumentNullException("You must provide two bots parameters for this action. Use '--a1' [--first] and '--a2' [--second] parameters");

            if (!OutputFilePath.IsSet)
                throw new ArgumentNullException("You must provide output file path parameter for this action. Use -o [--output] parameter.");


            IBlipAIClient bot1BlipAIClient = new BlipHttpClientAsync(Authorization1.Value);
            IBlipAIClient bot2BlipAIClient = new BlipHttpClientAsync(Authorization2.Value);

            var bot1Entities = await bot1BlipAIClient.GetAllEntities(verbose: IsVerbose);
            var bot1Intents = await bot1BlipAIClient.GetAllIntents(verbose: IsVerbose);

            var bot2Entities = await bot2BlipAIClient.GetAllEntities(verbose: IsVerbose);
            var bot2Intents = await bot2BlipAIClient.GetAllIntents(verbose: IsVerbose);

            foreach (var item1 in bot1Intents)
            {
                if (item1.Questions == null) continue;
                foreach (var question1 in item1.Questions)
                {
                    foreach (var item2 in bot2Intents)
                    {
                        if (item2.Questions == null) continue;
                        foreach (var question2 in item2.Questions)
                        {
                            int distance = _stringService.LevenshteinDistance(question1.Text, question2.Text);
                            if (distance <= 4)
                            {
                                Console.WriteLine($"{item1.Name} is close to {item2.Name}");
                                Console.WriteLine($"\tLev({question1.Text},{question2.Text}) = {distance}");
                            }
                        }
                    }
                }
            }
            return 0;
        }

        internal ComparisonMethod CustomMethodParser(string arg)
        {
            var defaultMethod = ComparisonMethod.Exact;

            if (string.IsNullOrWhiteSpace(arg)) return defaultMethod;

            var getType = TryGetContentType(arg);
            if (getType.HasValue)
            {
                return getType.Value;
            }

            return defaultMethod;
        }

        private ComparisonMethod? TryGetContentType(string content)
        {
            var validContents = Enum.GetNames(typeof(ComparisonMethod));
            var validContent = validContents.FirstOrDefault(c => c.ToLowerInvariant().Equals(content.ToLowerInvariant()));

            if (validContent != null)
                return Enum.Parse<ComparisonMethod>(validContent);

            return null;
        }
    }

    public enum ComparisonMethod
    {
        Exact,
        Levenshtein
    }

}
