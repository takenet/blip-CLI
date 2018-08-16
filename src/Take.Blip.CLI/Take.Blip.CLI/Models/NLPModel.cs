using System;
using System.Collections.Generic;
using System.Text;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Models
{
    internal class NLPModel
    {
        public string BotId { get; set; }
        public List<Entity> Entities { get; set; }
        public List<Intention> Intents { get; set; }
    }
}
