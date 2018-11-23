using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Models;
using Take.ContentProvider.Domain.Contract.Model;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface IContentManagerApiClient
    {
        Task<ContentManagerContentResult> GetAnswerAsync(string input, List<Tag> tags = null);
        Task<ContentManagerContentResult> GetAnswerAsync(string intent, List<string> entities, List<Tag> tags = null);
    }
}
