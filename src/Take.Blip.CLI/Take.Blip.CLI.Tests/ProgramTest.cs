using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI;
using Take.BlipCLI.Services.Interfaces;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.Blip.CLI.Tests
{
    [TestFixture]
    public class ProgramTest
    {
        [Test, NonParallelizable]
        public void Help_Command()
        {
            var args = new string[] { "help" };
            int result = Program.Main(args);
            Assert.AreEqual(result, 0);
        }

        [Test, NonParallelizable]
        public void Copy_Command()
        {
            //Arrange
            var args = new string[] { "copy", "--ta", "bla", "--fa", "bla", "-c", "AIModel" };
            var serviceProvider = Program.GetServiceCollection();

            var fromKey = "key1";
            var toKey = "key2";

            var sourceBlipAIClient = Substitute.For<IBlipAIClient>();
            sourceBlipAIClient.GetAllIntentsAsync(Arg.Any<bool>()).Returns(Task.FromResult<List<Intention>>(null));
            sourceBlipAIClient.GetAllEntities(Arg.Any<bool>()).Returns(Task.FromResult<List<Entity>>(null));

            var targetBlipAIClient = Substitute.For<IBlipAIClient>();
            targetBlipAIClient.GetAllIntentsAsync(Arg.Any<bool>()).Returns(Task.FromResult<List<Intention>>(null));
            targetBlipAIClient.GetAllEntities(Arg.Any<bool>()).Returns(Task.FromResult<List<Entity>>(null));

            var blipAIClientFactory = Substitute.For<IBlipClientFactory>();
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(fromKey))).Returns(sourceBlipAIClient);
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(toKey))).Returns(targetBlipAIClient);
            serviceProvider.AddSingleton<IBlipClientFactory>(blipAIClientFactory);
            Program.ServiceProvider = serviceProvider.BuildServiceProvider();

            //Act
            int result = Program.Main(args);

            //Assert
            Assert.AreEqual(result, 0);
        }

        [Test, NonParallelizable]
        public void Analyse_Command()
        {
            //Arrange
            var authKey = "key1";
            var input = "file.txt";
            var output = "result.txt";
            var args = new string[] { "analyse", "-a", authKey, "-i", input, "-o", output };
            var serviceProvider = Program.GetServiceCollection();

            var analysisResponse = new AnalysisResponse
            {
                Id = Guid.NewGuid().ToString(),
                Text = input,
                Intentions = new List<IntentionResponse> { new IntentionResponse { Id = "a", Score = 0.5f } }.ToArray(),
                Entities = new List<EntityResponse> { new EntityResponse { Id = "e", Value = "v" } }.ToArray()
            };
            var inputList = new List<string> { "a", "b", "c" };
            var blipAIClient = Substitute.For<IBlipAIClient>();
            blipAIClient.AnalyseForMetrics(Arg.Any<string>()).Returns(Task.FromResult(analysisResponse));
            var blipAIClientFactory = Substitute.For<IBlipClientFactory>();
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(authKey))).Returns(blipAIClient);
            var fileService = Substitute.For<IFileManagerService>();
            fileService.IsDirectory(input).Returns(true);
            fileService.IsFile(input).Returns(true);
            fileService.GetInputsToAnalyseAsync(input).Returns(inputList);

            serviceProvider.AddSingleton<IBlipClientFactory>(blipAIClientFactory);
            serviceProvider.AddSingleton<IFileManagerService>(fileService);

            Program.ServiceProvider = serviceProvider.BuildServiceProvider();

            //Act
            int result = Program.Main(args);

            //Assert
            Assert.AreEqual(result, 0);
        }

        [Test, NonParallelizable]
        public void Export_NLP_Command()
        {
            //Arrange
            var authKey = "key";
            var output = @"D:\path\to\file";
            var model = "NLPModel";
            var args = new string[] { "export", "-v", "-a", authKey, "-o", output, "-m", model };
            var serviceProvider = Program.GetServiceCollection();
            
            var sourceBlipAIClient = Substitute.For<IBlipAIClient>();
            sourceBlipAIClient.GetAllIntentsAsync(Arg.Any<bool>()).Returns(Task.FromResult<List<Intention>>(null));
            sourceBlipAIClient.GetAllEntities(Arg.Any<bool>()).Returns(Task.FromResult<List<Entity>>(null));
            var blipClientFactory = Substitute.For<IBlipClientFactory>();
            blipClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(authKey))).Returns(sourceBlipAIClient);
            var fileManagerService = Substitute.For<IFileManagerService>();
            var csvService = Substitute.For<ICSVGeneratorService>();

            serviceProvider.AddSingleton<IBlipClientFactory>(blipClientFactory);
            serviceProvider.AddSingleton<IFileManagerService>(fileManagerService);
            serviceProvider.AddSingleton<ICSVGeneratorService>(csvService);

            Program.ServiceProvider = serviceProvider.BuildServiceProvider();

            //Act
            int result = Program.Main(args);

            //Assert
            Assert.AreEqual(result, 0);
        }

    }
}
