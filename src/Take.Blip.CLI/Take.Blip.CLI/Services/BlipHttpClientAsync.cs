﻿using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Lime.Protocol.Serialization.Newtonsoft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Handlers;
using Take.BlipCLI.Services.Interfaces;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Services
{
    public class BlipHttpClientAsync : IBlipBucketClient, IBlipAIClient
    {
        private string _authorizationKey;
        private HttpClient _client = new HttpClient();
        private JsonNetSerializer _envelopeSerializer;

        public BlipHttpClientAsync(string authorizationKey)
        {
            _envelopeSerializer = new JsonNetSerializer();
            _authorizationKey = authorizationKey;
            _client.BaseAddress = new Uri("https://msging.net");
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", authorizationKey);
        }

        public async Task<AnalysisResponse> AnalyseForMetrics(string analysisRequest)
        {
            if (analysisRequest == null) throw new ArgumentNullException(nameof(analysisRequest));

            try
            {
                var command = new Command
                {
                    Id = EnvelopeId.NewId(),
                    To = Node.Parse("postmaster@ai.msging.net"),
                    Uri = new LimeUri("/analysis"),
                    Method = CommandMethod.Set,
                    Resource = new AnalysisRequest
                    {
                        TestingRequest = true,
                        Text = analysisRequest
                    }
                };

                var envelopeResult = await RunCommandAsync(command);

                return envelopeResult.Resource as AnalysisResponse;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return null;
            }
        }

        public async Task<bool> PingAsync(string node)
        {
            string validNode = node;

            if (!node.Contains("@"))
            {
                validNode = $"{node}@msging.net";
            }

            var command = new Command
            {
                Id = EnvelopeId.NewId(),
                To = Node.Parse(validNode),
                Uri = new LimeUri("/ping"),
                Method = CommandMethod.Get,
            };

            var envelopeResult = await RunCommandAsync(command);

            return Ping.MediaType.Equals(envelopeResult.Type) && envelopeResult.Status == CommandStatus.Success;
        }

        public async Task AddDocumentAsync(string key, Document document, BucketNamespace bucketNamespace = BucketNamespace.Document)
        {
            try
            {
                string @namespace;
                switch (bucketNamespace)
                {
                    case BucketNamespace.Document:
                        @namespace = "buckets";
                        break;

                    case BucketNamespace.Resource:
                        @namespace = "resources";
                        break;

                    case BucketNamespace.Profile:
                        @namespace = "profile";
                        break;

                    default:
                        @namespace = "buckets";
                        break;
                }

                var command = new Command
                {
                    Id = EnvelopeId.NewId(),
                    To = Node.Parse("postmaster@msging.net"),
                    Uri = new LimeUri($"/{@namespace}/{key}"),
                    Method = CommandMethod.Set,
                    Resource = document
                };

                var envelopeResult = await RunCommandAsync(command);
                EnsureCommandSuccess(envelopeResult);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public async Task<DocumentCollection> GetAllDocumentKeysAsync(BucketNamespace bucketNamespace = BucketNamespace.Document)
        {
            string @namespace;
            switch (bucketNamespace)
            {
                case BucketNamespace.Document:
                    @namespace = "buckets";
                    break;

                case BucketNamespace.Resource:
                    @namespace = "resources";
                    break;

                case BucketNamespace.Profile:
                    @namespace = "profile";
                    break;
                default:
                    @namespace = "buckets";
                    break;
            }

            try
            {
                var command = new Command
                {
                    Id = EnvelopeId.NewId(),
                    To = Node.Parse("postmaster@msging.net"),
                    Uri = new LimeUri($"/{@namespace}/"),
                    Method = CommandMethod.Get
                };

                var envelopeResult = await RunCommandAsync(command);
                return envelopeResult.Resource as DocumentCollection;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return null;
            }
        }

        public async Task<IEnumerable<KeyValuePair<string, Document>>> GetAllDocumentsAsync(DocumentCollection keysCollection, BucketNamespace bucketNamespace)
        {
            if (keysCollection.Total == 0) return null;

            try
            {
                string @namespace;
                switch (bucketNamespace)
                {
                    case BucketNamespace.Document:
                        @namespace = "buckets";
                        break;

                    case BucketNamespace.Resource:
                        @namespace = "resources";
                        break;

                    case BucketNamespace.Profile:
                        @namespace = "profile";
                        break;

                    default:
                        @namespace = "buckets";
                        break;
                }

                var pairsCollection = new List<KeyValuePair<string, Document>>();

                foreach (var key in keysCollection.Items)
                {
                    var command = new Command
                    {
                        Id = EnvelopeId.NewId(),
                        To = Node.Parse("postmaster@msging.net"),
                        Uri = new LimeUri($"/{@namespace}/{key}"),
                        Method = CommandMethod.Get
                    };

                    var envelopeResult = await RunCommandAsync(command);
                    var document = envelopeResult.Resource;

                    pairsCollection.Add(new KeyValuePair<string, Document>(key.ToString(), document));
                }

                return pairsCollection;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return null;
            }
        }

        public async Task AddEntity(string entityId)
        {
            try
            {
                var command = new Command
                {
                    Id = EnvelopeId.NewId(),
                    To = Node.Parse("postmaster@ai.msging.net"),
                    Uri = new LimeUri($"/entities/{entityId}"),
                    Method = CommandMethod.Delete,
                };

                await RunCommandAsync(command);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public async Task DeleteIntent(string intentId)
        {
            try
            {
                var command = new Command
                {
                    Id = EnvelopeId.NewId(),
                    To = Node.Parse("postmaster@ai.msging.net"),
                    Uri = new LimeUri($"/intentions/{intentId}"),
                    Method = CommandMethod.Delete,
                };

                await RunCommandAsync(command);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public async Task DeleteEntity(string entityId)
        {
            try
            {
                var command = new Command
                {
                    Id = EnvelopeId.NewId(),
                    To = Node.Parse("postmaster@ai.msging.net"),
                    Uri = new LimeUri($"/entities/{entityId}"),
                    Method = CommandMethod.Delete,
                };

                await RunCommandAsync(command);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public async Task<string> AddIntent(string intentName)
        {
            try
            {
                var command = new Command
                {
                    Id = EnvelopeId.NewId(),
                    To = Node.Parse("postmaster@ai.msging.net"),
                    Uri = new LimeUri("/intentions"),
                    Method = CommandMethod.Set,
                    Resource = new Intention
                    {
                        Name = intentName,
                    }
                };

                var envelopeResult = await RunCommandAsync(command);
                var createdIntention = envelopeResult.Resource as Intention;

                return createdIntention.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message: {0} ", e.Message);
                return null;
            }
        }

        public async Task AddQuestions(string intentId, Question[] questions)
        {
            if (questions == null) throw new ArgumentNullException(nameof(questions));

            try
            {
                var command = new Command
                {
                    Id = EnvelopeId.NewId(),
                    To = Node.Parse("postmaster@ai.msging.net"),
                    Uri = new LimeUri($"/intentions/{intentId}/questions"),
                    Method = CommandMethod.Set,
                    Resource = new DocumentCollection
                    {
                        ItemType = Question.MediaType,
                        Items = questions
                    }
                };

                await RunCommandAsync(command);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public async Task<List<Entity>> GetAllEntities(bool verbose = false)
        {
            var entitiesList = new List<Entity>();

            try
            {
                var command = new Command
                {
                    Id = EnvelopeId.NewId(),
                    To = Node.Parse("postmaster@ai.msging.net"),
                    Uri = new LimeUri($"/entities"),
                    Method = CommandMethod.Get,
                };

                LogVerbose(verbose, "Entities: ");

                var envelopeResult = await RunCommandAsync(command);
                var entities = envelopeResult.Resource as DocumentCollection ?? new DocumentCollection { Items = Enumerable.Empty<Document>().ToArray() };

                LogVerbose(verbose, $"{entities.Total} - ");

                foreach (var entity in entities)
                {
                    LogVerbose(verbose, "*");
                    entitiesList.Add(entity as Entity);
                }

                LogVerbose(verbose, "|");
                return entitiesList;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return null;
            }
        }

        public async Task<List<Intention>> GetAllIntents(bool verbose = false)
        {
            var intentsList = new List<Intention>();

            try
            {
                var command = new Command
                {
                    Id = EnvelopeId.NewId(),
                    To = Node.Parse("postmaster@ai.msging.net"),
                    Uri = new LimeUri($"/intentions?$take=500"),
                    Method = CommandMethod.Get,
                };

                var commandBase = new Command
                {
                    To = Node.Parse("postmaster@ai.msging.net"),
                    Method = CommandMethod.Get,
                };
                

                LogVerbose(verbose, "Intents: ");

                var envelopeResult = await RunCommandAsync(command);
                var intents = envelopeResult.Resource as DocumentCollection;

                LogVerbose(verbose, $"{intents.Total} - ");

                foreach (var intent in intents)
                {
                    LogVerbose(verbose, "*");
                    var intention = intent as Intention;

                    //Answers
                    var uri = Uri.EscapeUriString($"/intentions/{intention.Id}/answers");
                    commandBase.Uri = new LimeUri(uri);
                    envelopeResult = await GetCommandResultAsync(commandBase);
                    if(envelopeResult.Status != CommandStatus.Failure)
                    {
                        var answers = envelopeResult.Resource as DocumentCollection;
                        intention.Answers = answers.Items.Select(i => i as Answer).ToArray();
                    }

                    //Questions
                    uri = Uri.EscapeUriString($"/intentions/{intention.Id}/questions");
                    commandBase.Uri = new LimeUri(uri);
                    envelopeResult = await GetCommandResultAsync(commandBase);
                    if (envelopeResult.Status != CommandStatus.Failure)
                    {
                        var questions = envelopeResult.Resource as DocumentCollection;
                        intention.Questions = questions.Items.Select(i => i as Question).ToArray();
                    }

                    intentsList.Add(intention);
                }

                LogVerbose(verbose, "|");
                return intentsList;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                return null;
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        private async Task<Command> GetCommandResultAsync(Command command)
        {
            command.Id = EnvelopeId.NewId();
            return await RunCommandAsync(command);
        }

        private async Task<Command> RunCommandAsync(Command command)
        {
            var commandString = _envelopeSerializer.Serialize(command);

            using (var httpContent = new StringContent(commandString, Encoding.UTF8, "application/json"))
            {

                var response = await _client.PostAsync("/commands", httpContent);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                return (Command)_envelopeSerializer.Deserialize(responseBody);
            }
        }

        private static void LogVerbose(bool verbose, string message)
        {
            if (verbose) Console.Write(message);
        }

        private static void EnsureCommandSuccess(Command envelopeResult)
        {
            if (envelopeResult.Status == CommandStatus.Failure)
                throw new Exception($"Command failed: {envelopeResult.Reason.Description}({envelopeResult.Reason.Code})");
        }

        public Task AddEntity(Entity entity)
        {
            throw new NotImplementedException();
        }
    }

}




