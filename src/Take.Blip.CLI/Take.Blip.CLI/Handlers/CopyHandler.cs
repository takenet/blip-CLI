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
using Take.BlipCLI.Models;
using Microsoft.Extensions.Logging;

namespace Take.BlipCLI.Handlers
{
    public class CopyHandler : HandlerAsync
    {
        public INamedParameter<string> FromAuthorization { get; set; }
        public INamedParameter<string> ToAuthorization { get; set; }
        public INamedParameter<List<BucketNamespace>> Contents { get; set; }


        private readonly ISettingsFile _settingsFile;

        private readonly IBlipClientFactory _blipClientFactory;

        public CopyHandler(IBlipClientFactory blipClientFactory, IInternalLogger logger) : base(logger)
        {
            _settingsFile = new SettingsFile();
            _blipClientFactory = blipClientFactory;
        }

        public async override Task<int> RunAsync(string[] args)
        {
            if ((!FromAuthorization.IsSet) || (!ToAuthorization.IsSet))
                throw new ArgumentNullException("You must provide from and to parameters for this action. Use '-f' [--from] (or '--fa' [--fromAuthorization]) and '-t' [--to] (or '--ta' [--toAuthorization]) parameters");

            string fromAuthorization = FromAuthorization.Value;
            string toAuthorization = ToAuthorization.Value;

            IBlipBucketClient sourceBlipBucketClient = _blipClientFactory.GetInstanceForBucket(fromAuthorization);
            IBlipBucketClient targetBlipBucketClient = _blipClientFactory.GetInstanceForBucket(toAuthorization);

            IBlipAIClient sourceBlipAIClient = _blipClientFactory.GetInstanceForAI(fromAuthorization);
            IBlipAIClient targetBlipAIClient = _blipClientFactory.GetInstanceForAI(toAuthorization);

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
            _logger.LogDebug($"COPY AIMODEL: {fromAuthorization} to {toAuthorization}");

            if (Force.IsSet)
            {
                _logger.LogDebug("COPY AIMODEL - Force MODE: Deleting all entities and intents from target");

                var tEntities = await targetBlipAIClient.GetAllEntities();
                var tIntents = await targetBlipAIClient.GetAllIntentsAsync(justIds: true);

                await DeleteEntitiesAsync(targetBlipAIClient, tEntities);
                await DeleteIntentsAsync(targetBlipAIClient, tIntents);

                _logger.LogDebug("COPY AIMODEL - Force MODE: Intents and Entities deleted");

            }

            _logger.LogDebug("COPY AIMODEL - Getting AI Model from source: START");
            var entities = await sourceBlipAIClient.GetAllEntities(verbose: Verbose.IsSet);
            var intents = await sourceBlipAIClient.GetAllIntentsAsync(verbose: Verbose.IsSet);
            _logger.LogDebug("COPY AIMODEL - Getting AI Model from source: FINISH");

            _logger.LogDebug("COPY AIMODEL - Copying AI Model to target: START");
            await CopyEntitiesAsync(targetBlipAIClient, entities);
            await CopyIntentsAsync(targetBlipAIClient, intents);
            _logger.LogDebug("COPY AIMODEL - Copying AI Model to target: FINISH");
            _logger.LogDebug("COPY AIMODEL - DONE");
        }

        private async Task CopyIntentsAsync(IBlipAIClient blipAIClient, List<Intention> intents)
        {
            if (intents == null || intents.Count <= 0)
            {
                _logger.LogDebug($"No Intents");
            }
            else
            {
                var counter = 0;
                _logger.LogTrace($"Copying intents");
                foreach (var intent in intents)
                {
                    intent.Name = intent.Name.RemoveDiacritics().RemoveSpecialCharacters();
                    var id = await blipAIClient.AddIntent(intent.Name, verbose: Verbose.IsSet);
                    if (!string.IsNullOrEmpty(id))
                    {
                        if (intent.Questions != null) await blipAIClient.AddQuestions(id, intent.Questions);
                        if (intent.Answers != null) await blipAIClient.AddAnswers(id, intent.Answers);
                    }
                    counter++;
                    _logger.LogTrace($"Copying intents: {counter}/{intents.Count}");
                }
                _logger.LogDebug($"Intents copied");
            }
        }

        private async Task CopyEntitiesAsync(IBlipAIClient blipAIClient, List<Entity> entities)
        {
            if (entities == null || entities.Count <= 0)
            {
                _logger.LogDebug($"No Entities");
            }
            else
            {
                var counter = 0;
                _logger.LogTrace($"Copying entities");
                foreach (var entity in entities)
                {
                    await blipAIClient.AddEntity(entity);
                    counter++;
                    _logger.LogTrace($"Copying entities: {counter}/{entities.Count}");
                }
                _logger.LogDebug($"Entities copied");
            }
        }

        private async Task DeleteIntentsAsync(IBlipAIClient blipAIClient, List<Intention> tIntents)
        {
            if (tIntents == null || tIntents.Count <= 0)
            {
                _logger.LogDebug($"No Intents");
            }
            else
            {
                _logger.LogTrace($"Deleting intents");
                var counter = 0;
                foreach (var intent in tIntents)
                {
                    await blipAIClient.DeleteIntent(intent.Id);
                    counter++;
                    _logger.LogTrace($"Deleting intents: {counter}/{tIntents.Count}");
                }
                _logger.LogDebug($"Intents deleted");
            }
        }

        private async Task DeleteEntitiesAsync(IBlipAIClient blipAIClient, List<Entity> tEntities)
        {
            if (tEntities == null || tEntities.Count <= 0)
            {
                _logger.LogDebug($"No Entities");
            }
            else
            {
                _logger.LogTrace($"Deleting entities");
                var counter = 0;
                foreach (var entity in tEntities)
                {
                    await blipAIClient.DeleteEntity(entity.Id);
                    counter++;
                    _logger.LogTrace($"Deleting entities: {counter}/{tEntities.Count}");
                }
                _logger.LogDebug($"Entities deleted");
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



    }


}
