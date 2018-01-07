using ITGlobal.CommandLine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Take.BlipCLI.Handlers
{
    public class CopyHandler : HandlerAsync
    {
        public INamedParameter<string> From { get; set; }
        public INamedParameter<string> To { get; set; }
        public INamedParameter<List<ContentType>> Contents { get; set; }

        public async override Task<int> RunAsync(string[] args)
        {

            return 0;
        }

        public List<ContentType> CustomParser(string contents)
        {
            var contentsList = new List<ContentType>();
            var contentsArray = contents.Split(',');

            foreach (var content in contentsArray)
            {
                var contentType = TryGetContentType(content);
                if(contentType.HasValue)
                {
                    contentsList.Add(contentType.Value);
                }
            }

            return contentsList;
        }

        private ContentType? TryGetContentType(string content)
        {
            var validContents = Enum.GetNames(typeof(ContentType));
            var validContent = validContents.FirstOrDefault(c => c.ToLowerInvariant().Equals(content.ToLowerInvariant()));

            if (validContent != null)
                return Enum.Parse<ContentType>(validContent);

            return null;
        }
    }



    public enum ContentType
    {
        Resource,
        Document,
        Profile,
        AIModel
    }
}
