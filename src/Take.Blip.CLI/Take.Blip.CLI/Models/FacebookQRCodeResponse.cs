using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Models
{
    public partial class FacebookQrCodeResponse
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
}
