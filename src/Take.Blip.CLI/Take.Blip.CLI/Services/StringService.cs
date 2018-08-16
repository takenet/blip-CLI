using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Services
{
    public class StringService : IStringService
    {
        public int LevenshteinDistance(string s1, string s2)
        {
            return Fastenshtein.Levenshtein.Distance(s1, s2);
        }
    }
}
