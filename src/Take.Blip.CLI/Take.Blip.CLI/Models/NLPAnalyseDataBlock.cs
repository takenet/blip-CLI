using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Services.Interfaces;
using Take.ContentProvider.Domain.Contract.Interfaces;
using Take.ContentProvider.Domain.Contract.Model;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Models
{
    class NLPAnalyseDataBlock
    {
        #region Required
        public int Id { get; private set; }
        public string Input { get; private set; }
        public IBlipAIClient AIClient { get; private set; }
        public string ReportOutputFile { get; private set; }
        #endregion

        #region Optional
        public bool DoContentCheck { get; private set; }
        public List<Intention> AllIntents { get; private set; }
        public IContentProvider ContentProvider { get; private set; }
        #endregion

        #region Results
        public AnalysisResponse NLPAnalysisResponse { get; internal set; }
        public ContentResult ContentFromProvider { get; internal set; }
        #endregion

        public static NLPAnalyseDataBlock GetInstance(int id, string input, IBlipAIClient aiClient, string reportOutput)
        {
            return new NLPAnalyseDataBlock
            {
                Id = id,
                Input = input,
                AIClient = aiClient,
                ReportOutputFile = reportOutput

            };
        }

        public static NLPAnalyseDataBlock GetInstance(int id, string input, IBlipAIClient aiClient, string reportOutput, bool doCheck, List<Intention> intents, IContentProvider provider )
        {
            var instance = GetInstance(id, input, aiClient, reportOutput);
            instance.DoContentCheck = doCheck;
            instance.AllIntents = intents;
            instance.ContentProvider = provider;
            return instance;
        }

    }
}
