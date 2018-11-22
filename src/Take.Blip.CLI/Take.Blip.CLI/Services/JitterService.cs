using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Models.NLPAnalyse;
using Take.BlipCLI.Services.Interfaces;

namespace Take.BlipCLI.Services
{
    public class JitterService : IJitterService
    {
        private readonly IUniformProbabilityChecker _probabilityChecker;

        public JitterService(IUniformProbabilityChecker probabilityChecker)
        {
            _probabilityChecker = probabilityChecker;
        }

        public Task<string> ApplyJitterAsync(string source, JitterType jitterType, double probabilityScaleFactor = 0.5, int maxWordSize = 15)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException(nameof(source));
            }

            var words = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var jittered = new StringBuilder();
            foreach (var word in words)
            {
                var jitteredWord = ExecuteWordJitter(word, jitterType, probabilityScaleFactor, maxWordSize);
                jittered.Append(jitteredWord);
                jittered.Append(" ");
            }

            return Task.FromResult(jittered.ToString().Trim());
        }

        private string ExecuteWordJitter(string word, JitterType jitterType, double probabilityScaleFactor, int maxWordSize)
        {
            double jitterProbability = CalculateWordJitterProbability(word, jitterType, probabilityScaleFactor, maxWordSize);
            var proceed = _probabilityChecker.Check(jitterProbability);

            if (!proceed)
                return word;
            
            switch (jitterType)
            {
                case JitterType.MutationSwitch:
                    return MutationSwitch(word);
                case JitterType.MutationReplace:
                    return MutationReplace(word);
                case JitterType.MutationRemove:
                    return MutationRemove(word);
                default:
                    return word;
            }
        }

        private string MutationRemove(string word)
        {
            var newWord = new StringBuilder();
            var pos = _probabilityChecker.GetIntegerBetween(0, word.Length);
            int i = 0;
            foreach (var ch in word.ToCharArray())
            {
                if (i != pos) newWord.Append(ch);
                i++;
            }
            return newWord.ToString();
        }

        private string MutationReplace(string word)
        {
            var newWord = new StringBuilder();
            (var pos1, var pos2) = _probabilityChecker.GetTwoSequentialIntegerBetween(0, word.Length);
            var ch1 = word[pos1];
            var ch2 = word[pos2];
            int i = 0;
            foreach (var ch in word.ToCharArray())
            {
                if (i == pos1) newWord.Append(ch2);
                else if (i == pos2) newWord.Append(ch2);
                else newWord.Append(ch);
                i++;
            }
            return newWord.ToString();
        }

        private string MutationSwitch(string word)
        {
            var newWord = new StringBuilder();
            (var pos1, var pos2) = _probabilityChecker.GetTwoSequentialIntegerBetween(0, word.Length);
            var ch1 = word[pos1];
            var ch2 = word[pos2];
            int i = 0;
            foreach (var ch in word.ToCharArray())
            {
                if (i == pos1) newWord.Append(ch2);
                else if (i == pos2) newWord.Append(ch1);
                else newWord.Append(ch);
                i++;
            }
            return newWord.ToString();
        }

        private double CalculateWordJitterProbability(string word, JitterType jitterType, double probabilityScaleFactor, int maxWordSize)
        {
            return probabilityScaleFactor * (word.Length / (double) maxWordSize);
        }
    }
}
