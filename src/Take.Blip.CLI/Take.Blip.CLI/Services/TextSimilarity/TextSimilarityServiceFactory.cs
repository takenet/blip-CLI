using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Handlers;
using Take.BlipCLI.Services.Interfaces;
using Take.BlipCLI.Services.TextSimilarity.Interfaces;

namespace Take.BlipCLI.Services.TextSimilarity
{
    public class TextSimilarityServiceFactory : ITextSimilarityServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public TextSimilarityServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ITextSimilarityService GetServiceByType(ComparisonMethod type)
        {
            switch (type)
            {
                case ComparisonMethod.Exact:
                    return (ITextSimilarityService)_serviceProvider.GetService(typeof(ExactTextSimilarityService));
                case ComparisonMethod.Levenshtein:
                    return (ITextSimilarityService)_serviceProvider.GetService(typeof(AdaptedLevenshteinTextSimilarityService));
                case ComparisonMethod.JaroWinklerAndConsine:
                    return (ITextSimilarityService)_serviceProvider.GetService(typeof(JaroWinklerAndConsineTextSimilarityService));
                default:
                    throw new ArgumentException($"Invalid parameter value {type}", nameof(type));
            }
        }
        
    }
}
