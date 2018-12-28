using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Models;

namespace Take.BlipCLI.Services.TextSimilarity.Interfaces
{
    public interface ITextSimilarityService
    {
        double CalculateDistance(string text1, string text2);
        double CalculateMinimumDistance(string text1, string text2, NLPModelComparationType textType);
    }
}
