using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Models
{
    public class NLPExcelExportModel
    {
        public string SheetName { get; set; }
        public string[] Columns { get; set; }
        public string[,] SheetValues { get; set; }
    }
}
