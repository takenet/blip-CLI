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
        public NLPModelComparationType Reason { get; set; }
        public List<NLPModelComparationResultReasonExample> Examples { get; set; }
    }

    public class NLPModelComparationResultReasonExample
    {
        public string Text { get; set; }
        public string Key1 { get; set; }
        public string Key2 { get; set; }

        internal bool CheckKey(string text1, string text2)
        {
            return (Key1.Equals(text1) && Key2.Equals(text2)) || (Key1.Equals(text2) && Key2.Equals(text1));
        }
    }

    public enum NLPModelComparationType
    {
        Question,
        QuestionInSameIntent,
        Answer,
        Name,
        Value,
        Synonymous
    }

}
