using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Models.NLPAnalyse;
using Take.BlipCLI.Services;
using Take.BlipCLI.Services.Interfaces;

namespace Take.Blip.CLI.Tests.Services
{
    [TestFixture]
    public class JitterServiceTest
    {
        [Test]
        public async Task Jitter_Mutation_Remove()
        {
            //Arrange
            //var propCheck = Substitute.For<IUniformProbabilityChecker>();
            IUniformProbabilityChecker propCheck = new UniformProbabilityChecker();
            var jitterService = new JitterService(propCheck);
            string test = "Oi, isso eh uma frase de teste";

            //Act
            var jitterResult = new HashSet<string>();
            for (int i = 0; i < 1000; i++)
            {
                jitterResult.Add(await jitterService.ApplyJitterAsync(test, JitterType.MutationRemove, 0.8));
            }
            var result = new List<string>(jitterResult);
            //Assert
            foreach (var item in result)
            {
                Assert.That(test.Length >= item.Length, () => { return $"t: {test.Length}, i: {item.Length}"; });
            }
        }

        [Test]
        public async Task Jitter_Mutation_Replace()
        {
            //Arrange
            //var propCheck = Substitute.For<IUniformProbabilityChecker>();
            IUniformProbabilityChecker propCheck = new UniformProbabilityChecker();
            var jitterService = new JitterService(propCheck);
            string test = "Oi, isso eh uma frase de teste";

            //Act
            var jitterResult = new HashSet<string>();
            for (int i = 0; i < 1000; i++)
            {
                jitterResult.Add(await jitterService.ApplyJitterAsync(test, JitterType.MutationReplace, 0.8));
            }
            var result = new List<string>(jitterResult);
            //Assert
            foreach (var item in result)
            {
                Assert.That(test.Length == item.Length);
            }
        }

        [Test]
        public async Task Jitter_Mutation_Switch()
        {
            //Arrange
            //var propCheck = Substitute.For<IUniformProbabilityChecker>();
            IUniformProbabilityChecker propCheck = new UniformProbabilityChecker();
            var jitterService = new JitterService(propCheck);
            string test = "Oi, isso eh uma frase de teste";

            //Act
            var jitterResult = new HashSet<string>();
            for (int i = 0; i < 1000; i++)
            {
                jitterResult.Add(await jitterService.ApplyJitterAsync(test, JitterType.MutationSwitch, 0.8));
            }
            var result = new List<string>(jitterResult);
            //Assert
            foreach (var item in result)
            {
                Assert.That(test.Length == item.Length);
            }
        }
    }
}
