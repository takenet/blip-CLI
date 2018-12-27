using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface ITextSimilarityService
    {
        float CalculateDistance(string text1, string text2);
        float CalculateMinimumDistance(string text1, string text2);
    }
}
