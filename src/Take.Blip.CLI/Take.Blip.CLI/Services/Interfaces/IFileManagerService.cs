using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface IFileManagerService : INLPAnalyseFileReader, INLPAnalyseFileWriter
    {
        bool IsFile(string pathToFile);
        bool IsDirectory(string pathToFile);
        void CreateDirectoryIfNotExists(string fullFileName);
    }
}
