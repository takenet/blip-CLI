using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Handlers
{
    public class BucketExportHandler : ExportHandler
    {
        public string Key { get; set; }

        public BucketExportHandler(IBlipClientFactory blipClientFactory, IExcelGeneratorService excelGeneratorService) : base(blipClientFactory, excelGeneratorService)
        {
        }

        public static BucketExportHandler GetInstance(ExportHandler eh)
        {
            return new BucketExportHandler(eh.BlipClientFactory, eh.ExcelGeneratorService)
            {
                Node = eh.Node,
                Authorization = eh.Authorization,
                OutputFilePath = eh.OutputFilePath,
                Model = eh.Model,
                Verbose = eh.Verbose
            };
        }

        public override async Task<int> RunAsync(string[] args)
        {
            string authorization = GetAuthorization();

            var blipClient = BlipClientFactory.GetInstanceForBucket(authorization);

            //var key = "blip_portal:builder_working_flow";

            LogVerboseLine("Bucket Export");

            var data = await blipClient.GetDocumentAsync(Key, BucketNamespace.Document);

            if (data.HasValue)
            {
                var asString = JsonConvert.SerializeObject(data.Value.Value);
                var flow = Path.Combine(OutputFilePath.Value, "bucket.json");
                Directory.CreateDirectory(OutputFilePath.Value);
                using (var fw = new StreamWriter(flow, false, Encoding.UTF8))
                {
                    await fw.WriteAsync(asString);
                }
            }

            LogVerboseLine("DONE");

            return 0;
        }
    }
}
