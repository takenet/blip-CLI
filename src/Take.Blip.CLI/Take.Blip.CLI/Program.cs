using ITGlobal.CommandLine;
using Lime.Protocol.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Take.BlipCLI.Handlers;
using Take.BlipCLI.Services;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI
{
    public class Program
    {
        private static ISwitch _verbose;
        private static INamedParameter<int> _count;
        private static ISwitch _paged;

        public static int Main(string[] args)
        {
            RegisterBlipTypes();

            return CLI.HandleErrors(() =>
            {
                var app = CLI.Parser();

                app.ExecutableName(typeof(Program).GetTypeInfo().Assembly.GetName().Name);
                app.FromAssembly(typeof(Program).GetTypeInfo().Assembly);
                app.HelpText("BLiP Command Line Interface");

                _verbose = app.Switch("v").Alias("verbose").HelpText("Enable verbose output.");

                var pingHandler = new PingHandler();
                var pingCommand = app.Command("ping");
                pingHandler.Node = pingCommand.Parameter<string>("n").Alias("node").HelpText("Node to ping");
                pingCommand.HelpText("Ping a specific bot (node)");
                pingCommand.Handler(pingHandler.Run);

                var copyHandler = new CopyHandler();
                var copyCommand = app.Command("copy");
                copyHandler.From = copyCommand.Parameter<string>("f").Alias("from").HelpText("Node (bot) source");
                copyHandler.To = copyCommand.Parameter<string>("t").Alias("to").HelpText("Node (bot) target");
                copyHandler.Contents = copyCommand.Parameter<List<ContentType>>("c").Alias("contents").HelpText("Define which contents will be copied").ParseUsing(copyHandler.CustomParser);
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

                var tableCmd = app.Command("table");
                _count = tableCmd.Parameter<int>("n").DefaultValue(2).HelpText("Row count");
                _paged = tableCmd.Switch("p").HelpText("Enable paging");
                tableCmd.HelpText("Run a 'table' demo");
                tableCmd.Handler(TableDemo);

                app.Command("progress")
                    .HelpText("Run a 'progressbar' demo")
                    .Handler(ProgressBarDemo);

                app.Command("spinner")
                    .HelpText("Run a 'spinner' demo")
                    .Handler(SpinnerDemo);

                app.Command("run")
                    .HelpText("Run an 'unconsumed arguments' demo")
                    .Handler(UnconsumedArgumentsDemo);

                app.HelpCommand();

                return app.Parse(args).Run();
            });
        }

        private static void RegisterBlipTypes()
        {
            TypeUtil.RegisterDocument<AnalysisResponse>();
            TypeUtil.RegisterDocument<Intention>();
            TypeUtil.RegisterDocument<Entity>();
        }

        struct Xyz
        {
            public int X, Y, Z;
        }

        private static int TableDemo(string[] args)
        {
            var n = _count.Value;
            var data = new List<Xyz>();
            for (var x = 0; x <= n; x++)
            {
                data.Add(new Xyz { X = x, Y = x, Z = x });
            }

            CLI.Table(
                data,
                table =>
                {
                    table.Title("XYZ data");
                    table.EnablePaging(_paged.IsSet);
                    table.Column("X", _ => _.X.ToString(), _ => ConsoleColor.Red);
                    table.Column("Y", _ => _.Y.ToString(), _ => ConsoleColor.Green);
                    table.Column("Z", _ => _.Z.ToString(), _ => ConsoleColor.Blue);
                });

            return 0;
        }

        private static int ProgressBarDemo(string[] args)
        {
            using (var ctrlC = CLI.CtrlC())
            {
                ctrlC.CancellationToken.Register(() =>
                {
                    Console.WriteLine("Cancelled!");
                });
                Task.Run(async () =>
                {
                    using (var progressBar = CLI.ProgressBar())
                    {
                        var descr = new[] { "preparing", "fetching", "pulling", "pushing" };
                        while (true)
                        {
                            var progress = 0;
                            foreach (var op in descr)
                            {
                                Console.WriteLine("Starting operation {0}...", op);

                                progressBar.SetState(text: op);
                                for (var i = 0; i < 25; i++)
                                {
                                    progress++;
                                    await Task.Delay(50);
                                    progressBar.SetState(progress);

                                    if (ctrlC.CancellationToken.IsCancellationRequested)
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }).Wait();
            }

            return 0;
        }

        private static int SpinnerDemo(string[] args)
        {
            using (var ctrlC = CLI.CtrlC())
            {
                ctrlC.CancellationToken.Register(() =>
                {
                    Console.WriteLine("Cancelled!");
                });
                Task.Run(async () =>
                {
                    using (var spinner = CLI.Spinner(""))
                    {
                        var descr = new[] { "preparing", "fetching", "pulling", "pushing" };
                        while (true)
                        {
                            var progress = 0;
                            foreach (var op in descr)
                            {
                                Console.WriteLine("Starting operation {0}...", op);

                                spinner.SetTitle(op);
                                for (var i = 0; i < 25; i++)
                                {
                                    progress++;
                                    await Task.Delay(50);

                                    if (ctrlC.CancellationToken.IsCancellationRequested)
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }).Wait();
            }

            return 0;
        }

        private static int UnconsumedArgumentsDemo(string[] args)
        {
            return 0;
        }
    }
}
