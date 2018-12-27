using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Services
{
    public class TextSimilarityServiceFactory : ITextSimilarityServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public TextSimilarityServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ITextSimilarityService GetServiceByType(TextSimilarityServiceType type)
        {
            switch (type)
            {
                case TextSimilarityServiceType.AdaptedLevenshtein:
                    return (AdaptedLevenshteinTextSimilarityService)_serviceProvider.GetService(typeof(AdaptedLevenshteinTextSimilarityService));
                case TextSimilarityServiceType.JaroWinglerAndConsine:
                    return (JaroWinglerAndConsineTextSimilarityService)_serviceProvider.GetService(typeof(JaroWinglerAndConsineTextSimilarityService));
                default:
                    throw new ArgumentException($"Invalid parameter value {type}", nameof(type));
            }
        }
        
    }
}
