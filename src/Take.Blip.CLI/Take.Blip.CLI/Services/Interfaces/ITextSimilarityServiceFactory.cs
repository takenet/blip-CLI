using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface ITextSimilarityServiceFactory
    {
        ITextSimilarityService GetServiceByType(TextSimilarityServiceType type);
    }

    public enum TextSimilarityServiceType
    {
        AdaptedLevenshtein = 1,
        JaroWinglerAndConsine = 2
    }
}
