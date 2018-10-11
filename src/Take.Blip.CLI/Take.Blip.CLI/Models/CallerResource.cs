using Lime.Protocol;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Take.BlipCLI.Models
{
    [DataContract]
    public class CallerResource : Document
    {
        public static MediaType MediaType = MediaType.Parse("application/vnd.iris.configuration+json");
        public CallerResource() : base(MediaType)
        {
        }

        [DataMember]
        public string Owner { get; set; }

        [DataMember]
        public string Caller { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}
