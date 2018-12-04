using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Services.Interfaces
{
    public interface IUniformProbabilityChecker
    {
        bool Check(double probability);
        double GetDouble();
        (int, int) GetTwoSequentialIntegerBetween(int start, int end);
        int GetIntegerBetween(int start, int end);
    }
}
