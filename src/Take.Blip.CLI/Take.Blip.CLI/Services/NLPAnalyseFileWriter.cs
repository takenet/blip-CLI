using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Models;
using Take.BlipCLI.Services.Interfaces;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Services
{
    public class NLPAnalyseFileWriter : INLPAnalyseFileWriter
    {
        public async Task WriteAnalyseReportAsync(NLPAnalyseReport analyseReport)
        {
            var sortedAnalysis = analyseReport.AnalysisResponses.OrderBy(a => a.Text);
            using (var writer = new StreamWriter(analyseReport.FullReportFileName))
            {
                await writer.WriteLineAsync("Text\tIntentionId\tIntentionScore\tEntities");
                foreach (var item in sortedAnalysis)
                {
                    await writer.WriteLineAsync(AnalysisResponseToString(item));
                }
            }
        }

        private string AnalysisResponseToString(AnalysisResponse analysis)
        {
            var intention = analysis.Intentions?[0];
            var entities = analysis.Entities;
            return $"{analysis.Text}\t{intention?.Id}\t{intention?.Score}\t{EntitiesToString(entities?.ToList())}";
        }

        private string EntitiesToString(List<EntityResponse> entities)
        {
            if (entities == null || entities.Count < 1)
                return string.Empty;
            var toString = string.Join(", ", entities.Select(e => $"{e.Id}:{e.Value}"));
            return toString;
        }
    }
}
