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
    public class NLPAnalyseFileService : IFileManagerService
    {
        public void CreateDirectoryIfNotExists(string fullFileName)
        {
            var path = Path.GetDirectoryName(fullFileName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public async Task<List<string>> GetInputsToAnalyseAsync(string pathToFile)
        {
            var inputToAnalyse = new List<string>();
            using (var reader = new StreamReader(pathToFile, detectEncodingFromByteOrderMarks: true))
            {
                var line = "";
                while ((line = (await reader.ReadLineAsync())) != null)
                {
                    line = line.Trim();
                    if (string.IsNullOrWhiteSpace(line))
                        continue;
                    inputToAnalyse.Add(line);
                }
            }
            inputToAnalyse = inputToAnalyse.Distinct().ToList();

            return inputToAnalyse;
        }

        public bool IsDirectory(string pathToFile)
        {
            return Directory.Exists(pathToFile);
        }

        public bool IsFile(string pathToFile)
        {
            return File.Exists(pathToFile);
        }

        public async Task WriteAnalyseReportAsync(NLPAnalyseReport analyseReport, bool append = false)
        {
            bool writeHeader = !File.Exists(analyseReport.FullReportFileName);
            using (var writer = new StreamWriter(analyseReport.FullReportFileName, append))
            {
                if(writeHeader) await writer.WriteLineAsync("Text\tIntentionId\tIntentionScore\tEntities\tAnswer");
                foreach (var item in analyseReport.ResultData)
                {
                    await writer.WriteLineAsync(AnalysisResponseToString(item));
                }
            }
        }

        private string AnalysisResponseToString(AnalysisResultData analysis)
        {
            var intention = analysis.Intent;
            var entities = analysis.Entities;
            return $"{analysis.Input}\t{intention}\t{analysis.Confidence:P}\t{entities}\t\"{analysis.Answer}\"";
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
