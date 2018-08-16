using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface IBlipAIClient
    {
        Task<string> AddIntent(string intentName, bool verbose = false);
        Task DeleteIntent(string intentId);
        Task AddQuestions(string intentId, Question[] questions);
        Task AddAnswers(string intentId, Answer[] answers);
        Task<string> AddEntity(Entity entity);
        Task DeleteEntity(string entityId);
        Task<List<Entity>> GetAllEntities(bool verbose = false);
        Task<List<Intention>> GetAllIntents(bool verbose = false, bool justIds = false);
    }
}
