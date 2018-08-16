using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Services
{
    public class BlipKeysFormater
    {
        public static string GetAuthorizationKey(string identifier, string accessKey)
        {
            //Decode Access Key
            var decodedAccessKey = Base64Encoder.Decode(accessKey);
            //identifier:decoded(accessKey)
            return Base64Encoder.Encode($"{identifier.ToLowerInvariant()}:{decodedAccessKey}");
        }

        public static string GetAccessKey(string authorizationKey)
        {
            //Decode Authorization
            var decodedAuthorization = Base64Encoder.Decode(authorizationKey);
            //Split ':'
            var decodedAccessKey = decodedAuthorization.Split(':')[1];
            //Encode [1]
            return Base64Encoder.Encode(decodedAccessKey);
        }
    }
}
