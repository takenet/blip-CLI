using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Services
{
    public class NLPAnalyseFileReader : INLPAnalyseFileReader
    {
        public Task<List<string>> GetInputsToAnalyseAsync(string pathToFile)
        {
            throw new NotImplementedException();
        }

        public bool IsDirectory(string pathToFile)
        {
            var isDirectory = Directory.Exists(pathToFile);
            var isFile = File.Exists(pathToFile);
            return isDirectory && !isFile;
        }

        public bool IsFile(string pathToFile)
        {
            var isDirectory = Directory.Exists(pathToFile);
            var isFile = File.Exists(pathToFile);
            return isDirectory && isFile;
        }
    }
}
