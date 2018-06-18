using ITGlobal.CommandLine;
using Lime.Protocol.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Reflection;
using Take.BlipCLI.Handlers;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI
{
    public class Program
    {
        private static ISwitch _verbose;
        private static ISwitch _force;

        public static int Main(string[] args)
        {
            RegisterBlipTypes();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IStringService, StringService>()
                .AddSingleton<NLPCompareHandler>()
                .BuildServiceProvider();

            return CLI.HandleErrors(() =>
            {
                var app = CLI.Parser();
                
                app.ExecutableName(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
                app.FromAssembly(typeof(Program).GetTypeInfo().Assembly);
                app.HelpText("BLiP Command Line Interface");

                _verbose = app.Switch("v").Alias("verbose").HelpText("Enable verbose output.");
                _force = app.Switch("force").HelpText("Enable force operation.");

                var pingHandler = new PingHandler();
                var pingCommand = app.Command("ping");
                pingHandler.Node = pingCommand.Parameter<string>("n").Alias("node").HelpText("Node to ping");
                pingCommand.HelpText("Ping a specific bot (node)");
                pingCommand.Handler(pingHandler.Run);

                var nlpImportHandler = new NLPImportHandler();
                var nlpImportCommand = app.Command("nlp-import");
                nlpImportHandler.Node = nlpImportCommand.Parameter<string>("n").Alias("node").HelpText("Node to receive the data");
                nlpImportHandler.Authorization = nlpImportCommand.Parameter<string>("a").Alias("authorization").HelpText("Node Authorization to receive the data");
                nlpImportHandler.EntitiesFilePath = nlpImportCommand.Parameter<string>("ep").Alias("entities").HelpText("Path to entities file");
                nlpImportHandler.IntentsFilePath = nlpImportCommand.Parameter<string>("ip").Alias("intents").HelpText("Path to intents file");
                nlpImportCommand.HelpText("Import intents and entities to a specific bot (node)");
                nlpImportCommand.Handler(nlpImportHandler.Run);

                var copyHandler = new CopyHandler();
                var copyCommand = app.Command("copy");
                copyHandler.From = copyCommand.Parameter<string>("f").Alias("from").HelpText("Node (bot) source");
                copyHandler.To = copyCommand.Parameter<string>("t").Alias("to").HelpText("Node (bot) target");
                copyHandler.FromAuthorization = copyCommand.Parameter<string>("fa").Alias("fromAuthorization").HelpText("Authorization key of source bot");
                copyHandler.ToAuthorization = copyCommand.Parameter<string>("ta").Alias("toAuthorization").HelpText("Authorization key of target bot");
                copyHandler.Contents = copyCommand.Parameter<List<BucketNamespace>>("c").Alias("contents").HelpText("Define which contents will be copied").ParseUsing(copyHandler.CustomNamespaceParser);
                copyHandler.Verbose = _verbose;
                copyHandler.Force = _force;
                copyCommand.HelpText("Copy data from source bot (node) to target bot (node)");
                copyCommand.Handler(copyHandler.Run);

                var saveNodeHandler = new SaveNodeHandler();
                var saveNodeCommand = app.Command("saveNode");
                saveNodeHandler.Node = saveNodeCommand.Parameter<string>("n").Alias("node").HelpText("Node (bot) to be saved");
                saveNodeHandler.AccessKey = saveNodeCommand.Parameter<string>("k").Alias("accessKey").HelpText("Node accessKey");
                saveNodeHandler.Authorization = saveNodeCommand.Parameter<string>("a").Alias("authorization").HelpText("Node authoriaztion header");
                saveNodeCommand.HelpText("Save a node (bot) to be used next");
                saveNodeCommand.Handler(saveNodeHandler.Run);

                var formatKeyHandler = new FormatKeyHandler();
                var formatKeyCommand = app.Command("formatKey").Alias("fk");
                formatKeyHandler.Identifier = formatKeyCommand.Parameter<string>("i").Alias("identifier").HelpText("Bot identifier").Required();
                formatKeyHandler.AccessKey = formatKeyCommand.Parameter<string>("k").Alias("accessKey").HelpText("Bot accessKey");
                formatKeyHandler.Authorization = formatKeyCommand.Parameter<string>("a").Alias("authorization").HelpText("Bot authoriaztion header");
                formatKeyCommand.HelpText("Show all valid keys for a bot");
                formatKeyCommand.Handler(formatKeyHandler.Run);

                var nlpAnalyseHandler = new NLPAnalyseHandler();
                var nlpAnalyseCommand = app.Command("nlp-analyse").Alias("analyse");
                nlpAnalyseHandler.Text = nlpAnalyseCommand.Parameter<string>("t").Alias("text").HelpText("Text to be analysed");
                nlpAnalyseHandler.Node = nlpAnalyseCommand.Parameter<string>("n").Alias("node").Alias("identifier").HelpText("Bot identifier");
                nlpAnalyseHandler.Node = nlpAnalyseCommand.Parameter<string>("a").Alias("accessKey").HelpText("Bot access key");
                nlpAnalyseCommand.HelpText("Analyse some text using a bot IA model");
                nlpAnalyseCommand.Handler(nlpAnalyseHandler.Run);

                var exportHandler = new ExportHandler();
                var exportCommand = app.Command("export").Alias("get");
                exportHandler.Node = exportCommand.Parameter<string>("n").Alias("node").HelpText("Node (bot) source");
                exportHandler.Authorization = exportCommand.Parameter<string>("a").Alias("authorization").HelpText("Authorization key of source bot");
                exportHandler.OutputFilePath = exportCommand.Parameter<string>("o").Alias("output").Alias("path").HelpText("Output file path");
                exportHandler.Model = exportCommand.Parameter<ExportModel>("m").Alias("model").HelpText("Model to be exported").ParseUsing(exportHandler.CustomParser);
                exportCommand.HelpText("Export some BLiP model");
                exportCommand.Handler(exportHandler.Run);

                var compareHandler = serviceProvider.GetService<NLPCompareHandler>();
                var compareCommand = app.Command("comp").Alias("compare");
                compareHandler.Authorization1 = compareCommand.Parameter<string>("a1").Alias("authorization1").Alias("first").HelpText("Authorization key of first bot");
                compareHandler.Bot1Path = compareCommand.Parameter<string>("p1").Alias("path1").Alias("firstpath").HelpText("Path of first bot containing exported model");
                compareHandler.Authorization2 = compareCommand.Parameter<string>("a2").Alias("authorization2").Alias("second").HelpText("Authorization key of second bot");
                compareHandler.Bot2Path = compareCommand.Parameter<string>("p2").Alias("path2").Alias("secondpath").HelpText("Path of second bot containing exported model");
                compareHandler.OutputFilePath = compareCommand.Parameter<string>("o").Alias("output").Alias("path").HelpText("Output file path");
                compareHandler.Method = compareCommand.Parameter<ComparisonMethod>("m").Alias("method").HelpText("Comparison method (exact, levenshtein)").ParseUsing(compareHandler.CustomMethodParser);
                compareHandler.Verbose = _verbose;
                compareCommand.HelpText("Compare two knowledgebases");
                compareCommand.Handler(compareHandler.Run);

                app.HelpCommand();

                return app.Parse(args).Run();
            });
        }

        private static void RegisterBlipTypes()
        {
            TypeUtil.RegisterDocument<AnalysisResponse>();
            
            TypeUtil.RegisterDocument<Intention>();
            TypeUtil.RegisterDocument<Answer>();
            TypeUtil.RegisterDocument<Question>();

            TypeUtil.RegisterDocument<Entity>();
        }
        
    }
}
