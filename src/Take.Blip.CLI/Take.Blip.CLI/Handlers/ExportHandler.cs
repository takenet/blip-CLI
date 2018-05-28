using ITGlobal.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Take.BlipCLI.Handlers
{
    public class ExportHandler : HandlerAsync
    {
        public INamedParameter<string> Node { get; set; }
        public INamedParameter<string> Authorization { get; set; }
        public INamedParameter<string> OutputFilePath { get; set; }
        public INamedParameter<ExportModel> Model { get; set; }

        public override async Task<int> RunAsync(string[] args)
        {
            if (!Node.IsSet && !Authorization.IsSet)
                throw new ArgumentNullException("You must provide the target bot (node) for this action. Use '-n' [--node] (or '-a' [--authorization]) parameters");

            if (!Model.IsSet)
                throw new ArgumentNullException("You must provide the model type. Use '-m' [--model] parameter");
            

            HandlerAsync handler = null;
            switch (Model.Value)
            {
                case ExportModel.NLPModel:
                    handler = new NLPExportHandler { Node = Node, Authorization = Authorization, OutputFilePath = OutputFilePath };
                    break;
                default:
                    break;
            }

            if(handler != null)
            {
                return await handler.RunAsync(args);
            }

            return -1;
        }

        public ExportModel CustomParser(string type)
        {
            var defaultType = ExportModel.NLPModel;

            if (string.IsNullOrWhiteSpace(type)) return defaultType;


            var getType = TryGetContentType(type);
            if (getType.HasValue)
            {
                return getType.Value;
            }

            return defaultType;
        }

        private ExportModel? TryGetContentType(string content)
        {
            var validContents = Enum.GetNames(typeof(ExportModel));
            var validContent = validContents.FirstOrDefault(c => c.ToLowerInvariant().Equals(content.ToLowerInvariant()));

            if (validContent != null)
                return Enum.Parse<ExportModel>(validContent);

            return null;
        }


    }

    public enum ExportModel
    {
        NLPModel
    }
}
