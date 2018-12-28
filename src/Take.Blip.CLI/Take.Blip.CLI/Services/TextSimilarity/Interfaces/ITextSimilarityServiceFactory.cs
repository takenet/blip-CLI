using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Handlers;

namespace Take.BlipCLI.Services.TextSimilarity.Interfaces
{
    public interface ITextSimilarityServiceFactory
    {
        ITextSimilarityService GetServiceByType(ComparisonMethod type);
    }

}
