using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Models;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface IContentManagerApiClient
    {
        Task<ContentManagerContentResult> GetAnswerAsync(string input);
        Task<ContentManagerContentResult> GetAnswerAsync(string intent, List<string> entities);
    }
}
