using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Models.NLPAnalyse
{
    public class ReportDataLine
    {
        public int Id { get; set; }
        public InputWithTags Input { get; set; }
        public string Intent { get; set; }
        public double? Confidence { get; set; }
        public string Entities { get; set; }
        public string Answer { get; set; }
    }
}
