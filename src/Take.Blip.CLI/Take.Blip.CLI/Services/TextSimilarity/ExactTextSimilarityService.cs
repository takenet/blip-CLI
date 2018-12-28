using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Models;
using Take.BlipCLI.Services.TextSimilarity.Interfaces;

namespace Take.BlipCLI.Services.TextSimilarity
{
    public class ExactTextSimilarityService : ITextSimilarityService
    {
        public double CalculateDistance(string text1, string text2)
        {
            return text1.Equals(text2, StringComparison.OrdinalIgnoreCase) ? 0 : 1;
        }

        public double CalculateMinimumDistance(string text1, string text2, NLPModelComparationType textType)
        {
            return 0;
        }
    }
}
