using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Services.Interfaces;
using Take.ContentProvider.Domain.Contract.Interfaces;
using Take.ContentProvider.Domain.Contract.Model;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Models.NLPAnalyse
{
    class DataBlock
    {
        #region Required
        public int Id { get; private set; }
        public InputWithTags Input { get; private set; }
        public IBlipAIClient AIClient { get; private set; }
        public IContentManagerApiClient ContentClient { get; private set; }
        public string ReportOutputFile { get; private set; }
        #endregion

        #region Optional
        public bool DoContentCheck { get; private set; }
        public List<Intention> AllIntents { get; private set; }
        public IContentProvider ContentProvider { get; private set; }
        public bool ShouldWriteRawContent { get; private set; }
        #endregion

        #region Results
        public AnalysisResponse NLPAnalysisResponse { get; internal set; }
        public ContentManagerContentResult ContentFromProvider { get; internal set; }
        #endregion

        public static DataBlock GetInstance(int id, InputWithTags input, IBlipAIClient aiClient, IContentManagerApiClient contentClient, string reportOutput)
        {
            return new DataBlock
            {
                Id = id,
                Input = input,
                AIClient = aiClient,
                ReportOutputFile = reportOutput,
                ContentClient = contentClient
            };
        }

        public static DataBlock GetInstance(int id, InputWithTags input, IBlipAIClient aiClient, IContentManagerApiClient contentClient, string reportOutput, bool doCheck, bool rawContent, List<Intention> intents)
        {
            var instance = GetInstance(id, input, aiClient, contentClient, reportOutput);
            instance.DoContentCheck = doCheck;
            instance.ShouldWriteRawContent = rawContent;
            instance.AllIntents = intents;
            return instance;
        }

        public override string ToString()
        {
            return $"{Id}:{Input}";
        }

    }
}
