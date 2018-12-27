using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Services
{
    public class JaroWinglerAndConsineTextSimilarityService : ITextSimilarityService
    {
        private readonly IStringService _stringService;

        public JaroWinglerAndConsineTextSimilarityService(IStringService stringService)
        {
            _stringService = stringService;
        }

        public float CalculateDistance(string text1, string text2)
        {
            return _stringService.LevenshteinDistance(text1, text2);
        }

        public float CalculateMinimumDistance(string text1, string text2)
        {
            int smallerStringSize = Math.Min(text1.Length, text1.Length);
            return (float)Math.Max(1, 2 * Math.Log(smallerStringSize));
        }
    }
}
