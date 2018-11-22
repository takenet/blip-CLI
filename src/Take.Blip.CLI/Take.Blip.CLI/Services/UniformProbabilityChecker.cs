using System;
using System.Collections.Generic;
using System.Text;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Services
{
    public class UniformProbabilityChecker : IUniformProbabilityChecker
    {
        public static Random RANDOM_GENERATOR = null;

        public UniformProbabilityChecker()
        {
            if(RANDOM_GENERATOR == null)
            {
                RANDOM_GENERATOR = new Random();
            }
        }

        public bool Check(double probability)
        {
            return RANDOM_GENERATOR.NextDouble() <= probability;
        }

        public int GetIntegerBetween(int start, int end)
        {
            return RANDOM_GENERATOR.Next(start, end);
        }

        public (int, int) GetTwoSequentialIntegerBetween(int start, int end)
        {
            var next = RANDOM_GENERATOR.Next(start, end - 1);
            return (next, next + 1);
        }
    }
}
