using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Lime.Protocol.Serialization.Newtonsoft;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Handlers;
using Take.BlipCLI.Services.Interfaces;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Services
{
    public class BlipHttpClientAsync : IBlipBucketClient
    {
        private string _authorizationKey;
        private HttpClient _client = new HttpClient();

        public BlipHttpClientAsync(string authorizationKey)
        {
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

                var envelopeSerializer = new JsonNetSerializer();
                var commandString = envelopeSerializer.Serialize(command);

                var httpContent = new StringContent(commandString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("/commands", httpContent);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                var envelopeResult = (Command)envelopeSerializer.Deserialize(responseBody);

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

            var envelopeSerializer = new JsonNetSerializer();
            var commandString = envelopeSerializer.Serialize(command);

            var httpContent = new StringContent(commandString, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync("/commands", httpContent);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var envelopeResult = (Command)envelopeSerializer.Deserialize(responseBody);

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

                var documentSerializer = new DocumentSerializer();

                var envelopeSerializer = new JsonNetSerializer();
                var commandString = envelopeSerializer.Serialize(command);

                var httpContent = new StringContent(commandString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("/commands", httpContent);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                //var envelopeResult = (Command)envelopeSerializer.Deserialize(responseBody);

            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        public void Dispose()
        {
            _client.Dispose();
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

                var documentSerializer = new DocumentSerializer();

                var envelopeSerializer = new JsonNetSerializer();
                var commandString = envelopeSerializer.Serialize(command);

                var httpContent = new StringContent(commandString, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync("/commands", httpContent);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                var envelopeResult = (Command)envelopeSerializer.Deserialize(responseBody);

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

                    var documentSerializer = new DocumentSerializer();

                    var envelopeSerializer = new JsonNetSerializer();
                    var commandString = envelopeSerializer.Serialize(command);

                    var httpContent = new StringContent(commandString, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await _client.PostAsync("/commands", httpContent);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    var envelopeResult = (Command)envelopeSerializer.Deserialize(responseBody);
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

    }

}




