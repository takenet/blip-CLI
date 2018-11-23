using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Extensions;
using Take.BlipCLI.Models;
using Take.BlipCLI.Models.NLPAnalyse;
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

        public async Task<List<InputWithTags>> GetInputsToAnalyseAsync(string pathToFile)
        {
            int totalLines = 0;
            int totalInputs = 0;
            int totalDistincts = 0;
            var inputsToAnalyse = new List<InputWithTags>();
            using (var reader = new StreamReader(pathToFile, detectEncodingFromByteOrderMarks: true))
            {
                var line = "";
                while ((line = (await reader.ReadLineAsync())) != null)
                {
                    totalLines++;
                    line = line.Trim();
                    if (string.IsNullOrWhiteSpace(line)) 
                        continue;
                    var inputWithTags = ConvertStringToInputWithTags(line);
                    totalInputs++;
                    inputsToAnalyse.Add(inputWithTags);
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

        public async Task WriteAnalyseReportAsync(Report analyseReport, bool append = false)
        {
            bool writeHeader = !File.Exists(analyseReport.FullReportFileName);
            using (var writer = new StreamWriter(analyseReport.FullReportFileName, append))
            {
                if(writeHeader) await writer.WriteLineAsync("Id\tText\tIntentionId\tIntentionScore\tEntities\tAnswer\tRawResponse");
                foreach (var item in analyseReport.ReportDataLines)
                {
                    await writer.WriteLineAsync(AnalysisResponseToString(item, analyseReport.WriteRawContent));
                }
            }
        }

        private InputWithTags ConvertStringToInputWithTags(string line)
        {
            /* Parts:
             * 0 - Input
             * 1 - Tags (0 = null)
             * 2 - Intent (0 = null)
             * 3 - Entities (0 = null)
             * 4 - Answer (0 = null)
             */
            var parts = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);

            var inputWithTags = new InputWithTags
            {
                Input = parts[0]
            };

            for (int i = 1; i < parts.Length; i++)
            {
                ProcessPartsToFillInputWithTags(parts, inputWithTags, i);
            }

            return inputWithTags;
        }

        private void ProcessPartsToFillInputWithTags(string[] parts, InputWithTags inputWithTags, int i)
        {
            switch (i)
            {
                case 1: // Tags
                    if (parts[i] == "0")
                    {
                        inputWithTags.Tags = null;
                    }
                    else
                    {
                        inputWithTags.Tags = InputWithTags.GetTagsByString(parts[i]).ToList();
                    }
                    break;
                case 2: // Intent
                    if (parts[i] == "0")
                    {
                        inputWithTags.IntentExpected = null;
                    }
                    else
                    {
                        inputWithTags.IntentExpected = parts[i];
                    }
                    break;
                case 3: // Entities
                    if (parts[i] == "0")
                    {
                        inputWithTags.EntitiesExpected = null;
                    }
                    else
                    {
                        inputWithTags.EntitiesExpected = parts[i].Split(',').ToList();
                    }
                    break;
                case 4: // Answer
                    if (parts[i] == "0")
                    {
                        inputWithTags.AnswerExpected = null;
                    }
                    else
                    {
                        inputWithTags.AnswerExpected = parts[i];
                    }
                    break;
            }
        }

        private string AnalysisResponseToString(ReportDataLine reportDataLine, bool writeRawContent)
        {
            var intention = reportDataLine.Intent;
            var entities = reportDataLine.Entities;
            var rawContent = writeRawContent ? reportDataLine.AnalysisResponse.Intentions.ToJson() : string.Empty;
            return $"{reportDataLine.Id}\t{reportDataLine.Input.Input}\t{intention}\t{reportDataLine.Confidence:N2}\t{entities}\t\"{reportDataLine.Answer}\"\t{rawContent}";
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
