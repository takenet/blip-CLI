using System;
using System.Collections.Generic;
using System.Text;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Models.NLPAnalyse
{
    public class Report
    {
        public string FullReportFileName { get; set; }
        public List<ReportDataLine> ReportDataLines { get; internal set; }
    }
}
