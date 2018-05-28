using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Services.Interfaces
{
    interface IBlipAIClient
    {
        Task<string> AddIntent(string intentName);
        Task DeleteIntent(string intentId);
        Task AddQuestions(string intentId, Question[] questions);
        Task AddEntity(Entity entity);
        Task<List<Entity>> GetAllEntities(bool verbose = false);
        Task<List<Intention>> GetAllIntents(bool verbose = false);
    }
}
