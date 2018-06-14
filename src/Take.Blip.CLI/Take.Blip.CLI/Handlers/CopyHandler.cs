﻿using ITGlobal.CommandLine;
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
        public ISwitch CleanTarget { get; set; }
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
                        LogVerbose("> Source data <");
                        var entities = await sourceBlipAIClient.GetAllEntities(Verbose.IsSet);
                        var intents = await sourceBlipAIClient.GetAllIntents(Verbose.IsSet);

                        if (CleanTarget.IsSet)
                        {
                            LogVerbose("> Remove target data <");
                            var targetEntities = await targetBlipAIClient.GetAllEntities(Verbose.IsSet);
                            foreach (var entity in targetEntities)
                            {
                                await targetBlipAIClient.DeleteEntity(entity.Id);
                            }

                            var targetIntents = await targetBlipAIClient.GetAllIntents(Verbose.IsSet);
                            foreach (var intent in targetIntents)
                            {
                                await targetBlipAIClient.DeleteIntent(intent.Id);
                            }
                        }

                        LogVerbose("> Target data <");
                        foreach (var entity in entities)
                        {
                            await targetBlipAIClient.AddEntity(entity);
                        }

                        foreach (var intent in intents)
                        {
                            var id = await targetBlipAIClient.AddIntent(intent.Name);
                            await targetBlipAIClient.AddQuestions(id, intent.Questions);
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
            if (Verbose.IsSet) Console.Write(message + Environment.NewLine);
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
