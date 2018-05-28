using ITGlobal.CommandLine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Take.BlipCLI.Services.Interfaces;
using Take.BlipCLI.Services;
using Lime.Protocol;
using Take.BlipCLI.Services.Settings;

namespace Take.BlipCLI.Handlers
{
    public class CopyHandler : HandlerAsync
    {
        public INamedParameter<string> From { get; set; }
        public INamedParameter<string> FromAuthorization { get; set; }
        public INamedParameter<string> To { get; set; }
        public INamedParameter<string> ToAuthorization { get; set; }
        public INamedParameter<List<BucketNamespace>> Contents { get; set; }
        public ISwitch Verbose { get; set; }
        public ISwitch Force { get; set; }

        private readonly ISettingsFile _settingsFile;

        public CopyHandler()
        {
            _settingsFile = new SettingsFile();
        }

        public async override Task<int> RunAsync(string[] args)
        {
            if ((!From.IsSet && !FromAuthorization.IsSet) || (!To.IsSet && !ToAuthorization.IsSet))
                throw new ArgumentNullException("You must provide from and to parameters for this action. Use '-f' [--from] (or '-fa' [--fromAuthorization]) and '-t' [--to] (or '-ta' [--toAuthorization]) parameters");

            string fromAuthorization = FromAuthorization.Value;
            string toAuthorization = ToAuthorization.Value;

            if (From.IsSet && string.IsNullOrEmpty(fromAuthorization))
            {
                fromAuthorization = _settingsFile.GetNodeCredentials(Node.Parse(From.Value)).Authorization;
            }

            if (To.IsSet && string.IsNullOrEmpty(toAuthorization))
            {
                toAuthorization = _settingsFile.GetNodeCredentials(Node.Parse(To.Value)).Authorization;
            }

            IBlipBucketClient sourceBlipBucketClient = new BlipHttpClientAsync(fromAuthorization);
            IBlipBucketClient targetBlipBucketClient = new BlipHttpClientAsync(toAuthorization);

            IBlipAIClient sourceBlipAIClient = new BlipHttpClientAsync(fromAuthorization);
            IBlipAIClient targetBlipAIClient = new BlipHttpClientAsync(toAuthorization);

            foreach (var content in Contents.Value)
            {
                //if IAModel handle in a different way
                if (content.Equals(BucketNamespace.AIModel))
                {
                    var entities = await sourceBlipAIClient.GetAllEntities();
                    var intents = await sourceBlipAIClient.GetAllIntents();

                    foreach (var entity in entities)
                    {
                        await targetBlipAIClient.AddEntity(entity);
                    }

                    foreach (var intent in intents)
                    {
                        var id = await targetBlipAIClient.AddIntent(intent.Name, verbose: Verbose.IsSet);
                        if (!string.IsNullOrEmpty(id))
                        {
                            await targetBlipAIClient.AddQuestions(id, intent.Questions);
                            await targetBlipAIClient.AddAnswers(id, intent.Answers);
                        }
                    }
                }
                else
                {
                    var documentKeysToCopy = await sourceBlipBucketClient.GetAllDocumentKeysAsync(content) ?? new DocumentCollection();
                    var documentPairsToCopy = await sourceBlipBucketClient.GetAllDocumentsAsync(documentKeysToCopy, content);

                    if (documentPairsToCopy != null)
                    {
                        foreach (var resourcePair in documentPairsToCopy)
                        {
                            await targetBlipBucketClient.AddDocumentAsync(resourcePair.Key, resourcePair.Value, content);
                        }
                    }
                }
            }

            return 0;
        }

        public List<BucketNamespace> CustomNamespaceParser(string contents)
        {
            var defaultContents = new List<BucketNamespace> {
                BucketNamespace.AIModel,
                BucketNamespace.Document,
                BucketNamespace.Profile,
                BucketNamespace.Resource
            };

            if (string.IsNullOrWhiteSpace(contents)) return defaultContents;

            var contentsList = new List<BucketNamespace>();
            var contentsArray = contents.Split(',');

            foreach (var content in contentsArray)
            {
                var contentType = TryGetContentType(content);
                if (contentType.HasValue)
                {
                    contentsList.Add(contentType.Value);
                }
            }

            if (contentsList.Count == 0) return defaultContents;

            return contentsList;
        }

        public bool CustomVerboseParser(string verbose)
        {
            return true;
        }

        private BucketNamespace? TryGetContentType(string content)
        {
            var validContents = Enum.GetNames(typeof(BucketNamespace));
            var validContent = validContents.FirstOrDefault(c => c.ToLowerInvariant().Equals(content.ToLowerInvariant()));

            if (validContent != null)
                return Enum.Parse<BucketNamespace>(validContent);

            return null;
        }
    }

    public enum BucketNamespace
    {
        Resource,
        Document,
        Profile,
        AIModel
    }
}
