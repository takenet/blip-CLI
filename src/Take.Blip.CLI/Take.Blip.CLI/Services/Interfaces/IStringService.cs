using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface IStringService
    {
        int LevenshteinDistance(string s1, string s2);

    }
}
