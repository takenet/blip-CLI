using F23.StringSimilarity;
using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Models;
using Take.BlipCLI.Services.Interfaces;
using Take.BlipCLI.Services.TextSimilarity.Interfaces;

namespace Take.BlipCLI.Services.TextSimilarity
{
    public class JaroWinklerAndConsineTextSimilarityService : ITextSimilarityService
    {
        private readonly IStringService _stringService;
        private readonly JaroWinkler _jaroWinklerMetric;
        private readonly Cosine _cosineMetric;

        public JaroWinklerAndConsineTextSimilarityService(
            IStringService stringService
            )
        {
            _stringService = stringService;
            _jaroWinklerMetric = new JaroWinkler();
            _cosineMetric = new Cosine();
        }

        public double CalculateDistance(string text1, string text2)
        {
            return Measure(text1, text2);
        }

        public double CalculateMinimumDistance(string text1, string text2, NLPModelComparationType textType)
        {
            if (textType == NLPModelComparationType.Name || textType == NLPModelComparationType.QuestionInSameIntent)
            {
                return 0.1d;
            }
            return 0.4d;
        }

        private double Measure(string text1, string text2)
        {
            return (_jaroWinklerMetric.Distance(text1, text2) +
                _cosineMetric.Distance(text1, text2)) * 0.5d;
        }
    }
}
