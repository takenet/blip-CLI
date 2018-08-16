using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Take.Blip.CLI.Tests.Models;
using Take.BlipCLI.Handlers;
using Take.BlipCLI.Services.Interfaces;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.Blip.CLI.Tests.Features.Analyse
{
    [TestFixture]
    public class NLPAnalyseTest
    {

        [Test]
        public void When_NLPAnalyse_Authorization_NotSet_Then_ThrowsException()
        {
            //Arrange
            var authKey = "key1";

            var blipAIClient = Substitute.For<IBlipAIClient>();
            blipAIClient.AnalyseForMetrics(Arg.Any<string>()).Returns(Task.FromResult<AnalysisResponse>(null));

            var blipAIClientFactory = Substitute.For<IBlipClientFactory>();
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(authKey))).Returns(blipAIClient);
            var fileService = Substitute.For<IFileManagerService>();

            var handler = new NLPAnalyseHandler(blipAIClientFactory, fileService)
            {
                Authorization = new MyNamedParameter<string> { Value = null },
                Input = new MyNamedParameter<string> { Value = null },
                Verbose = new MySwitch { IsSet = false }
            };

            //Act
            void Act()
            {
                var status = handler.RunAsync(null).Result;
            }

            //Assert
            Exception e = null;
            try
            {
                Act();
            }
            catch (AggregateException ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.AreEqual(1, (e as AggregateException).InnerExceptions.Count);
            Assert.IsInstanceOf(typeof(ArgumentNullException), e.InnerException);
        }

        [Test]
        public void When_NLPAnalyse_Input_NotSet_Then_ThrowsException()
        {
            //Arrange
            var authKey = "key1";

            var blipAIClient = Substitute.For<IBlipAIClient>();
            blipAIClient.AnalyseForMetrics(Arg.Any<string>()).Returns(Task.FromResult<AnalysisResponse>(null));

            var blipAIClientFactory = Substitute.For<IBlipClientFactory>();
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(authKey))).Returns(blipAIClient);
            var fileService = Substitute.For<IFileManagerService>();

            var handler = new NLPAnalyseHandler(blipAIClientFactory, fileService)
            {
                Authorization = new MyNamedParameter<string> { Value = authKey },
                Input = new MyNamedParameter<string> { Value = null },
                ReportOutput = new MyNamedParameter<string> { Value = string.Empty },
                Verbose = new MySwitch { IsSet = false }
            };

            //Act
            void Act()
            {
                var status = handler.RunAsync(null).Result;
            }

            //Assert
            Exception e = null;
            try
            {
                Act();
            }
            catch (AggregateException ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.AreEqual(1, (e as AggregateException).InnerExceptions.Count);
            Assert.IsInstanceOf(typeof(ArgumentNullException), e.InnerException);
        }

        [Test]
        public void When_NLPAnalyse_Input_IsEmpty_Then_ThrowsException()
        {
            //Arrange
            var authKey = "key1";

            var blipAIClient = Substitute.For<IBlipAIClient>();
            blipAIClient.AnalyseForMetrics(Arg.Any<string>()).Returns(Task.FromResult<AnalysisResponse>(null));

            var blipAIClientFactory = Substitute.For<IBlipClientFactory>();
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(authKey))).Returns(blipAIClient);
            var fileService = Substitute.For<IFileManagerService>();

            var handler = new NLPAnalyseHandler(blipAIClientFactory, fileService)
            {
                Authorization = new MyNamedParameter<string> { Value = authKey },
                Input = new MyNamedParameter<string> { Value = string.Empty },
                ReportOutput = new MyNamedParameter<string> { Value = string.Empty },
                Verbose = new MySwitch { IsSet = false }
            };

            //Act
            void Act()
            {
                var status = handler.RunAsync(null).Result;
            }

            //Assert
            Exception e = null;
            try
            {
                Act();
            }
            catch (AggregateException ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.AreEqual(1, (e as AggregateException).InnerExceptions.Count);
            Assert.IsInstanceOf(typeof(ArgumentNullException), e.InnerException);
        }

        [Test]
        public void When_NLPAnalyse_Input_IsDirectory_Then_ThrowsException()
        {
            //Arrange
            var authKey = "key1";
            var input = "C:";
            var blipAIClient = Substitute.For<IBlipAIClient>();
            blipAIClient.AnalyseForMetrics(Arg.Any<string>()).Returns(Task.FromResult<AnalysisResponse>(null));

            var blipAIClientFactory = Substitute.For<IBlipClientFactory>();
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(authKey))).Returns(blipAIClient);
            var fileService = Substitute.For<IFileManagerService>();
            fileService.IsDirectory(input).Returns(true);
            fileService.IsFile(input).Returns(false);

            var handler = new NLPAnalyseHandler(blipAIClientFactory, fileService)
            {
                Authorization = new MyNamedParameter<string> { Value = authKey },
                Input = new MyNamedParameter<string> { Value = input },
                ReportOutput = new MyNamedParameter<string> { Value = string.Empty },
                Verbose = new MySwitch { IsSet = false }
            };

            //Act
            void Act()
            {
                var status = handler.RunAsync(null).Result;
            }

            //Assert
            Exception e = null;
            try
            {
                Act();
            }
            catch (AggregateException ex)
            {
                e = ex;
            }
            Assert.IsNotNull(e);
            Assert.AreEqual(1, (e as AggregateException).InnerExceptions.Count);
            Assert.IsInstanceOf(typeof(ArgumentNullException), e.InnerException);
        }

        [Test]
        public void When_NLPAnalyse_Input_IsPhrase_Then_AnalysePhrase()
        {
            //Arrange
            var authKey = "key1";
            var input = "This is a phrase";
            var analysisResponse = new AnalysisResponse
            {
                Id = Guid.NewGuid().ToString(),
                Text = input
            };
            var blipAIClient = Substitute.For<IBlipAIClient>();
            blipAIClient.AnalyseForMetrics(Arg.Any<string>()).Returns(Task.FromResult(analysisResponse));

            var blipAIClientFactory = Substitute.For<IBlipClientFactory>();
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(authKey))).Returns(blipAIClient);
            var fileService = Substitute.For<IFileManagerService>();

            var handler = new NLPAnalyseHandler(blipAIClientFactory, fileService)
            {
                Authorization = new MyNamedParameter<string> { Value = authKey },
                Input = new MyNamedParameter<string> { Value = input },
                ReportOutput = new MyNamedParameter<string> { Value = string.Empty },
                Verbose = new MySwitch { IsSet = false }
            };

            //Act
            var status = handler.RunAsync(null).Result;

            //Assert
            blipAIClient.Received(1).AnalyseForMetrics(input);
        }

        [Test]
        public void When_NLPAnalyse_Input_IsFile_Then_AnalyseFile()
        {
            //Arrange
            var authKey = "key1";
            var input = "file.txt";
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

            var handler = new NLPAnalyseHandler(blipAIClientFactory, fileService)
            {
                Authorization = new MyNamedParameter<string> { Value = authKey },
                Input = new MyNamedParameter<string> { Value = input },
                ReportOutput = new MyNamedParameter<string> { Value = string.Empty },
                Verbose = new MySwitch { IsSet = false }
            };

            //Act
            var status = handler.RunAsync(null).Result;

            //Assert
            foreach (var item in inputList)
            {
                blipAIClient.Received().AnalyseForMetrics(item);
            }
            blipAIClient.Received(inputList.Count).AnalyseForMetrics(Arg.Any<string>());
        }
    }
}
