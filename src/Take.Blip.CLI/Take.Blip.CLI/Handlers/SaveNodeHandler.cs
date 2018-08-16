using ITGlobal.CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Settings;

namespace Take.BlipCLI.Handlers
{
    public class SaveNodeHandler : HandlerAsync
    {
        public INamedParameter<string> Node { get; set; }
        public INamedParameter<string> AccessKey { get; set; }
        public INamedParameter<string> Authorization { get; set; }
        private readonly ISettingsFile _settingsFile;

        public SaveNodeHandler()
        {
            _settingsFile = new SettingsFile();
        }

        public async override Task<int> RunAsync(string[] args)
        {
            if (!AccessKey.IsSet && !Authorization.IsSet)
                throw new ArgumentNullException("You must provide at least one of the key parameters for a bot. Use 'k' or 'a' parameters");

            if (!Node.IsSet)
                throw new ArgumentNullException("You must provide the node or a bot identifier using 'n' parameter. For example: 'saveNode -n papagaio -k YOUR KEY' or 'saveNode -n papagaio@msging.net -k YOUR KEY'");

            var stringNode = Node.Value.ToLowerInvariant().Trim();
            if (!stringNode.Contains("@")) stringNode += "@msging.net";

            var node = Lime.Protocol.Node.Parse(stringNode);

            var authorization = Authorization.Value;
            var accessKey = AccessKey.Value;

            if (!Authorization.IsSet)
            {
                authorization = BlipKeysFormater.GetAuthorizationKey(node.Name, accessKey);
            }

            _settingsFile.AddNodeCredentials(new NodeCredential { Node = node, Authorization = authorization });
            
            return 0;
        }
    }
}
