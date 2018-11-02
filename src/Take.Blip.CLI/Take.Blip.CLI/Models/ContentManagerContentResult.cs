using System;
using System.Collections.Generic;
using System.Text;
using Take.ContentProvider.Domain.Contract.Model;

namespace Take.BlipCLI.Models
{
    public class ContentManagerContentResult
    {
        public List<Content> Contents { get; set; }

        public int Status { get; set; }

        public ContentManagerNLPAnalyseResponse NLPResponse { get; set; }
    }

    public class ContentManagerNLPAnalyseResponse
    {
        public string Intent { get; set; }
        public double? IntentScore { get; set; }
        public List<string> Entities { get; set; }

    }
}
