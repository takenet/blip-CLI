using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface ITextSimilarityService
    {
        double CalculateDistance(string text1, string text2);
        double CalculateMinimumDistance(string text1, string text2);
    }
}
