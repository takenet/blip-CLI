using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Models.NLPAnalyse
{
    public class AnalyseParameters
    {
        public string Authorization { get; set; }
        public string InputSource { get; set; }
        public string ReportOutput { get; set; }
        public bool DoContentCheck { get; set; }
        public bool ShowRawContent { get; set; }
        public int JitterSize { get; set; }

    }
}
