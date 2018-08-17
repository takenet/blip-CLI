using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Models
{
    public class NLPExportModel
    {
        public string Name { get; set; }
        public string[] Columns { get; set; }
        public string[,] Values { get; set; }
    }
}
