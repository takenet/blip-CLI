using System;
using System.Collections.Generic;
using System.Text;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Models
{
    public class NLPAnalyseReport
    {
        public string FullReportFileName { get; set; }
        public List<NLPAnalyseReportDataLine> ReportDataLines { get; internal set; }
    }
}
