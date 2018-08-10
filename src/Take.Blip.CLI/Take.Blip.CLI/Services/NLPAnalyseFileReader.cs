using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Services
{
    public class NLPAnalyseFileReader : INLPAnalyseFileReader
    {
        public async Task<List<string>> GetInputsToAnalyseAsync(string pathToFile)
        {
            var inputToAnalyse = new List<string>();
            using (var reader = new StreamReader(pathToFile))
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
    }
}
