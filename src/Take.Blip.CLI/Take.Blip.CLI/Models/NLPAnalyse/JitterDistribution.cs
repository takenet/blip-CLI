using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Models.NLPAnalyse
{
    public class JitterDistribution
    {
        public double MutationSwitch { get; private set; }
        public double MutationReplace { get; private set; }
        public double MutationRemove { get; private set; }
        public double MutationDuplicate { get; private set; }

        private JitterDistribution() { }

        public static JitterDistribution NewDistribution(double mswitch, double mreplace, double mremove, double mduplicate)
        {
            if ( Math.Abs(mswitch + mreplace + mremove + mduplicate - 1f) > 0.001f)
            {
                throw new ArgumentException("Arguments sum must be 1.0f");
            }

            return new JitterDistribution
            {
                MutationSwitch = mswitch,
                MutationReplace = mreplace,
                MutationRemove = mremove,
                MutationDuplicate = mduplicate
            };
        }


    }
}
