using ITGlobal.CommandLine;
using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;
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
        public ISwitch Force { get; set; }
        public ISwitch Verbose { get; set; }

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

            try
            {
                foreach (var content in Contents.Value)
                {
                    //if IAModel handle in a different way
                    if (content.Equals(BucketNamespace.AIModel))
                    {
                        LogVerboseLine($"COPY AIMODEL: {From.Value} to {To.Value}");

                        if (Force.IsSet)
                        {
                            LogVerboseLine("FORCE mode");
                            LogVerboseLine("\t> Deleting all entities and intents from target");

                            var targetEntities = await targetBlipAIClient.GetAllEntities(Verbose.IsSet);
                            LogVerbose($"\t>>> Entities: {targetEntities.Count} - ");
                            foreach (var entity in targetEntities)
                            {
                                await targetBlipAIClient.DeleteEntity(entity.Id);
                                LogVerbose("*");
                            }

                            LogVerboseLine("|");

                            var targetIntents = await targetBlipAIClient.GetAllIntents(Verbose.IsSet);
                            LogVerbose($"\t>>> Intent: {targetIntents.Count} - ");
                            foreach (var intent in targetIntents)
                            {
                                await targetBlipAIClient.DeleteIntent(intent.Id);
                                LogVerbose("*");
                            }
                            LogVerboseLine("|");
                        }

                        LogVerboseLine("COPY: ");
                        LogVerboseLine("\t> Getting AI Model from source: ");
                        LogVerbose("\t>>> ");
                        var entities = await sourceBlipAIClient.GetAllEntities(Verbose.IsSet);
                        LogVerbose("\t>>> ");
                        var intents = await sourceBlipAIClient.GetAllIntents(Verbose.IsSet);

                        LogVerboseLine("\t> Copying AI Model to target: ");
                        LogVerbose($"\t>>> Entities: {entities.Count} - ");
                        foreach (var entity in entities)
                        {
                            await targetBlipAIClient.AddEntity(entity);
                            LogVerbose("*");
                        }

                        LogVerboseLine("|");

                        LogVerbose($"\t>>> Intents: {intents.Count} - ");
                        foreach (var intent in intents)
                        {
                            intent.Name = intent.Name.RemoveDiacritics().RemoveSpecialCharacters();
                            var id = await targetBlipAIClient.AddIntent(intent.Name, verbose: Verbose.IsSet);
                            if (!string.IsNullOrEmpty(id))
                            {
                                if (intent.Questions != null) await targetBlipAIClient.AddQuestions(id, intent.Questions);
                                if (intent.Answers != null) await targetBlipAIClient.AddAnswers(id, intent.Answers);
                            }
                            LogVerbose("*");
                        }

                        LogVerboseLine("|");
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
            catch (Exception e)
            {
                Console.WriteLine("\n>> Failed!");
                Console.WriteLine(e);
                return 1;
            }
        }

        public List<BucketNamespace> CustomParser(string contents)
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

            if(contentsList.Count == 0) return defaultContents;

            return contentsList;
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
