using ITGlobal.CommandLine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Handlers;
using Take.BlipCLI.Services.Interfaces;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;

namespace Take.Blip.Tests.Features.Copy
{
    [TestClass]
    public class CopyNLPModelTest
    {


        [TestMethod]
        public void Scenario1Test()
        {
            //Arrange
            var fromKey = "key1";
            var toKey = "key2";

            var sourceBlipAIClient = Substitute.For<IBlipAIClient>();
            sourceBlipAIClient.GetAllEntities(Arg.Any<bool>()).Returns(Task.FromResult<List<Entity>>(null));
            sourceBlipAIClient.GetAllIntents(Arg.Any<bool>()).Returns(Task.FromResult<List<Intention>>(null));

            var targetBlipAIClient = Substitute.For<IBlipAIClient>();
            var blipAIClientFactory = Substitute.For<IBlipAIClientFactory>();
            blipAIClientFactory.GetInstance(Arg.Is<string>(s => s.Equals(fromKey))).Returns(sourceBlipAIClient);
            blipAIClientFactory.GetInstance(Arg.Is<string>(s => s.Equals(toKey))).Returns(targetBlipAIClient);


            var handler = new CopyHandler(blipAIClientFactory);
            handler.FromAuthorization = new MyNamedParameter<string> { Value = fromKey };
            handler.ToAuthorization = new MyNamedParameter<string> { Value = toKey };
            handler.From = new MyNamedParameter<string>();
            handler.To = new MyNamedParameter<string>();
            handler.Verbose = new MySwitch { IsSet = false };
            handler.Force = new MySwitch { IsSet = false };
            handler.Contents = new MyNamedParameter<List<BucketNamespace>> { Value = new List<BucketNamespace> { BucketNamespace.AIModel } };
            //Act
            handler.RunAsync(null).Wait();


            //Assert


        }

    }


    class MyNamedParameter<T> : INamedParameter<T>
    {
        public bool IsSet => Value != null;

        public T Value { set; get; }

        public event Action<INamedParameter<T>> ValueParsed;

        public INamedParameter<T> Alias(string name)
        {
            throw new NotImplementedException();
        }

        public INamedParameter<T> DefaultValue(T value)
        {
            throw new NotImplementedException();
        }

        public INamedParameter<T> HelpText(string text)
        {
            throw new NotImplementedException();
        }

        public INamedParameter<T> Hidden(bool hidden = true)
        {
            throw new NotImplementedException();
        }

        public INamedParameter<T> ParseUsing(Func<string, T> parser)
        {
            throw new NotImplementedException();
        }

        public INamedParameter<T> Required(bool isRequired = true)
        {
            throw new NotImplementedException();
        }
    }

    class MySwitch : ISwitch
    {
        public bool IsSet { get; set; }

        public event Action<ISwitch> ValueParsed;

        public ISwitch Alias(string name)
        {
            throw new NotImplementedException();
        }

        public ISwitch HelpText(string text)
        {
            throw new NotImplementedException();
        }

        public ISwitch Hidden(bool hidden = true)
        {
            throw new NotImplementedException();
        }
    }

}
