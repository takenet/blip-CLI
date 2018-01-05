using ITGlobal.CommandLine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Services;

namespace Take.BlipCLI.Handlers
{
    public class FormatKeyHandler : HandlerAsync
    {
        public INamedParameter<string> Identifier { get; set; }
        public INamedParameter<string> AccessKey { get; set; }
        public INamedParameter<string> Authorization { get; set; }

        public override async Task<int> RunAsync(string[] args)
        {
            var authorization = Authorization.Value;
            var accessKey = AccessKey.Value;

            if (!AccessKey.IsSet && !Authorization.IsSet)
                throw new ArgumentNullException("You must provide at least one of the key parameters for a bot. Use 'k' or 'a' parameters");

            if (AccessKey.IsSet)
            {
                //Decode Access Key
                var decodedAccessKey = Base64Encoder.Decode(accessKey);
                //identifier:decoded(accessKey)
                authorization = Base64Encoder.Encode($"{Identifier.Value.ToLowerInvariant()}:{decodedAccessKey}");
            }
            else
            {
                //Decode Authorization
                var decodedAuthorization = Base64Encoder.Decode(authorization);
                //Split ':'
                var decodedAccessKey = decodedAuthorization.Split(':')[1];
                //Encode [1]
                accessKey = Base64Encoder.Encode(decodedAccessKey);
            }

            Console.WriteLine($"Identifier: {Identifier.Value.ToLowerInvariant()}\n");

            Console.Write("AccessKey: ");
            using (CLI.WithForeground(ConsoleColor.Blue))
            {
                Console.Write($"{accessKey}\n");
            }
            Console.Write("Authorization: ");
            using (CLI.WithForeground(ConsoleColor.Blue))
            {
                Console.Write($"{authorization}\n");
            }

            return 0;
        }
    }
}
