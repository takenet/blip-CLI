using ITGlobal.CommandLine;
using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;
using Take.BlipCLI.Services.Settings;
using System.Globalization;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Handlers
{
    public class CopyHandler : HandlerAsync
    {
        public INamedParameter<string> From { get; set; }
        public INamedParameter<string> FromAuthorization { get; set; }
        public INamedParameter<string> To { get; set; }
        public INamedParameter<string> ToAuthorization { get; set; }
        public INamedParameter<List<BucketNamespace>> Contents { get; set; }
        public ISwitch Force { get; set; }
        public ISwitch Verbose { get; set; }

        private readonly ISettingsFile _settingsFile;

        private readonly IBlipAIClientFactory _blipAIClientFactory;

        public CopyHandler(IBlipAIClientFactory blipAIClientFactory)
        {
            _settingsFile = new SettingsFile();
            _blipAIClientFactory = blipAIClientFactory;
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

            IBlipAIClient sourceBlipAIClient = _blipAIClientFactory.GetInstance(fromAuthorization);
            IBlipAIClient targetBlipAIClient = _blipAIClientFactory.GetInstance(toAuthorization);

            foreach (var content in Contents.Value)
            {
                //if IAModel handle in a different way
                if (content.Equals(BucketNamespace.AIModel))
                {
                    await CopyAIModelAsync(fromAuthorization, toAuthorization, sourceBlipAIClient, targetBlipAIClient);
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

        private async Task CopyAIModelAsync(string fromAuthorization, string toAuthorization, IBlipAIClient sourceBlipAIClient, IBlipAIClient targetBlipAIClient)
        {
            LogVerboseLine($"COPY AIMODEL: {fromAuthorization} to {toAuthorization}");

            if (Force.IsSet)
            {
                LogVerboseLine("Force MODE");
                LogVerboseLine("\t> Deleting all entities and intents from target");

                var tEntities = await targetBlipAIClient.GetAllEntities();
                var tIntents = await targetBlipAIClient.GetAllIntents(justIds: true);

                await DeleteEntitiesAsync(targetBlipAIClient, tEntities);
                await DeleteIntentsAsync(targetBlipAIClient, tIntents);
            }

            LogVerboseLine("COPY: ");
            LogVerboseLine("\t> Getting AI Model from source: ");
            LogVerbose("\t>>> ");
            var entities = await sourceBlipAIClient.GetAllEntities(verbose: Verbose.IsSet);
            LogVerbose("\t>>> ");
            var intents = await sourceBlipAIClient.GetAllIntents(verbose: Verbose.IsSet);

            LogVerboseLine("\t> Copying AI Model to target: ");

            await CopyEntitiesAsync(targetBlipAIClient, entities);
            await CopyIntentsAsync(targetBlipAIClient, intents);

            LogVerbose($"\t> DONE");
        }

        private async Task CopyIntentsAsync(IBlipAIClient blipAIClient, List<Intention> intents)
        {
            if (intents == null || intents.Count <= 0)
            {
                LogVerbose($"\t>>> No Intents");
            }
            else
            {
                LogVerbose($"\t>>> Intents: {intents.Count} - ");
                foreach (var intent in intents)
                {
                    intent.Name = intent.Name.RemoveDiacritics().RemoveSpecialCharacters();
                    var id = await blipAIClient.AddIntent(intent.Name, verbose: Verbose.IsSet);
                    if (!string.IsNullOrEmpty(id))
                    {
                        if (intent.Questions != null) await blipAIClient.AddQuestions(id, intent.Questions);
                        if (intent.Answers != null) await blipAIClient.AddAnswers(id, intent.Answers);
                    }
                    LogVerbose("*");
                }
                LogVerboseLine("|");
            }
        }

        private async Task CopyEntitiesAsync(IBlipAIClient blipAIClient, List<Entity> entities)
        {
            if (entities == null || entities.Count <= 0)
            {
                LogVerbose($"\t>>> No Entities");
            }
            else
            {
                LogVerbose($"\t>>> Entities: {entities.Count} - ");
                foreach (var entity in entities)
                {
                    await blipAIClient.AddEntity(entity);
                    LogVerbose("*");
                }
                LogVerboseLine("|");
            }
        }

        private async Task DeleteIntentsAsync(IBlipAIClient blipAIClient, List<Intention> tIntents)
        {
            if (tIntents == null || tIntents.Count <= 0)
            {
                LogVerbose($"\t>>> No Intent");
            }
            else
            {
                LogVerbose($"\t>>> Intent: {tIntents.Count} - ");
                foreach (var intent in tIntents)
                {
                    await blipAIClient.DeleteIntent(intent.Id);
                    LogVerbose("*");
                }
                LogVerboseLine("|");
            }
        }

        private async Task DeleteEntitiesAsync(IBlipAIClient blipAIClient, List<Entity> tEntities)
        {
            if (tEntities == null || tEntities.Count <= 0)
            {
                LogVerbose($"\t>>> No Entities");
            }
            else
            {
                LogVerbose($"\t>>> Entities: {tEntities.Count} - ");
                foreach (var entity in tEntities)
                {
                    await blipAIClient.DeleteEntity(entity.Id);
                    LogVerbose("*");
                }
                LogVerboseLine("|");
            }
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


        private BucketNamespace? TryGetContentType(string content)
        {
            var validContents = Enum.GetNames(typeof(BucketNamespace));
            var validContent = validContents.FirstOrDefault(c => c.ToLowerInvariant().Equals(content.ToLowerInvariant()));

            if (validContent != null)
                return Enum.Parse<BucketNamespace>(validContent);

            return null;
        }

        private void LogVerbose(string message)
        {
            if (Verbose.IsSet) Console.Write(message);
        }

        private void LogVerboseLine(string message)
        {
            if (Verbose.IsSet) Console.WriteLine(message);
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
