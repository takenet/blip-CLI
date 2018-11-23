using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Models;
using Take.BlipCLI.Models.NLPAnalyse;
using Take.BlipCLI.Services.Interfaces;
using Take.ContentProvider.Domain.Contract.Model;

namespace Take.BlipCLI.Services
{
    public class ContentManagerApiClient : IContentManagerApiClient
    {
        private string _authorizationKey;
        private HttpClient _client = new HttpClient();

        public ContentManagerApiClient(string authorizationKey)
        {
            _authorizationKey = authorizationKey;
            _client.BaseAddress = new Uri("https://az-infobots.take.net");
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Key", authorizationKey);
        }

        public async Task<ContentManagerContentResult> GetAnswerAsync(string input, List<Tag> tags = null)
        {
            var encodedInput = Uri.EscapeDataString(input);
            using (var response = await _client.GetAsync($"/answer?input={encodedInput}"))
            {
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ContentManagerContentResult>(responseBody);
            }
        }

        public async Task<ContentManagerContentResult> GetAnswerAsync(string intent, List<string> entities, List<Tag> tags = null)
        {
            try
            {
                var localIntent = Uri.EscapeDataString(intent) ?? string.Empty;
                var localEntities = entities ?? new List<string>();
                localEntities = localEntities.Select(e => Uri.EscapeDataString(e)).ToList();
                var uri = $"/contentproviderapi/content/test?intent={localIntent}";
                if (localEntities.Count > 0)
                    uri = $"{uri}&entities={string.Join(',', localEntities)}";

                var localTags = tags.Select(e => Uri.EscapeDataString($"{e.Key}:{e.Value}")).ToList();
                if(tags != null && tags.Any())
                {
                    uri = $"{uri}&tags={string.Join(',', localTags)}";
                }

                using (var response = await _client.GetAsync(uri))
                {
                    response.EnsureSuccessStatusCode();
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ContentManagerContentResult>(responseBody);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
