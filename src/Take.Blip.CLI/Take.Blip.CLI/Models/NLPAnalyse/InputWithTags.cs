using System;
using System.Collections.Generic;
using System.Text;
using Take.ContentProvider.Domain.Contract.Model;

namespace Take.BlipCLI.Models.NLPAnalyse
{
    public class InputWithTags
    {
        public string Input { get; private set; }
        public List<Tag> Tags { get; set; }
        public string IntentExpected { get; set; }
        public List<string> EntitiesExpected { get; set; }
        public string AnswerExpected { get; set; }


    }
}
