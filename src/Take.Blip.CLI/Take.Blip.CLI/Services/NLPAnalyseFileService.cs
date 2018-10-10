using Microsoft.Extensions.Logging;
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
        private readonly IInternalLogger _logger;

        public NLPAnalyseFileService(IInternalLogger logger)
        {
            _logger = logger;
        }

        public void CreateDirectoryIfNotExists(string fullFileName)
        {
            var path = Path.GetDirectoryName(fullFileName);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        public async Task<List<string>> GetInputsToAnalyseAsync(string pathToFile)
        {
            int totalLines = 0;
            int totalInputs = 0;
            int totalDistincts = 0;
            var inputsToAnalyse = new List<string>();
            using (var reader = new StreamReader(pathToFile, detectEncodingFromByteOrderMarks: true))
            {
                var line = "";
                while ((line = (await reader.ReadLineAsync())) != null)
                {
                    totalLines++;
                    line = line.Trim();
                    if (string.IsNullOrWhiteSpace(line)) 
                        continue;
                    totalInputs++;
                    inputsToAnalyse.Add(line);
                }
            }
            inputsToAnalyse = inputsToAnalyse.Distinct().ToList();
            totalDistincts = inputsToAnalyse.Count;

            _logger.LogDebug($"Distinct/Inputs: {totalDistincts}/{totalInputs} - Total Lines: {totalLines}");

            return inputsToAnalyse;
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
                if(writeHeader) await writer.WriteLineAsync("Id\tText\tIntentionId\tIntentionScore\tEntities\tAnswer");
                foreach (var item in analyseReport.ReportDataLines)
                {
                    await writer.WriteLineAsync(AnalysisResponseToString(item));
                }
            }
        }

        private string AnalysisResponseToString(NLPAnalyseReportDataLine reportDataLine)
        {
            var intention = reportDataLine.Intent;
            var entities = reportDataLine.Entities;
            return $"{reportDataLine.Id}\t{reportDataLine.Input}\t{intention}\t{reportDataLine.Confidence:P}\t{entities}\t\"{reportDataLine.Answer}\"";
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
