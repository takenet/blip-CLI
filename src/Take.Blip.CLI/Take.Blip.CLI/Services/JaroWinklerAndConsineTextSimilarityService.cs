using F23.StringSimilarity;
using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Services
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
            return _stringService.LevenshteinDistance(text1, text2);
        }

        public double CalculateMinimumDistance(string text1, string text2)
        {
            int smallerStringSize = Math.Min(text1.Length, text1.Length);
            return Math.Max(1, 2 * Math.Log(smallerStringSize));
        }

        private double Measure(string text1, string text2)
        {
            return (_jaroWinklerMetric.Distance(text1, text2) + 
                _cosineMetric.Distance(text1, text2)) * 0.5d;
        }
    }
}
