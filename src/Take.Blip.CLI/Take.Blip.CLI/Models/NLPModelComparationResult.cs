using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Models
{
    public class NLPModelComparationResult
    {
        public string Element1 { get; set; }
        public string Element2 { get; set; }
        public List<NLPModelComparationResultReason> Reasons { get; set; }

        internal bool CheckKey(string text1, string text2)
        {
            return (Element1.Equals(text1) && Element2.Equals(text2)) || (Element1.Equals(text2) && Element2.Equals(text1));
        }
    }

    public class NLPModelComparationResultReason
    {
        public NLPModelComparationResultReasonType Reason { get; set; }
        public List<string> Examples { get; set; }
    }

    public enum NLPModelComparationResultReasonType
    {
        Question,
        Answer,
        Name,
        Value,
        Synonymous
    }

}
