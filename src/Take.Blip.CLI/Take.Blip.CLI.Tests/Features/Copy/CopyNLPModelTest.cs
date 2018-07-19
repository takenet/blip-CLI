using ITGlobal.CommandLine;
using NSubstitute;
using NUnit.Framework;
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
    [TestFixture]
    public class CopyNLPModelTest
    {

        /**
         * Testes:
         * De um modelo, para um modelo
         * Com 0 intenção
         * Com 1 intenção
         * Com N intenções
         * Com 0 entidades
         * Com 1 entidades
         * Com N entidades
         * Modelo completo
         * 
         */
        [Test]
        public void Scenario1Test()
        {
            //Arrange
            var fromKey = "key1";
            var toKey = "key2";

            var sourceBlipAIClient = Substitute.For<IBlipAIClient>();
            sourceBlipAIClient.GetAllEntities(Arg.Any<bool>()).Returns(Task.FromResult<List<Entity>>(null));
            sourceBlipAIClient.GetAllIntents(Arg.Any<bool>()).Returns(Task.FromResult<List<Intention>>(null));

            var targetBlipAIClient = Substitute.For<IBlipAIClient>();
            var blipAIClientFactory = Substitute.For<IBlipClientFactory>();
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(fromKey))).Returns(sourceBlipAIClient);
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(toKey))).Returns(targetBlipAIClient);


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

        [Test]
        public void Copy_EmptyModel_To_Other_EmptyModel_WithoutForce()
        {
            //Arrange
            var fromKey = "key1";
            var toKey = "key2";

            var sourceBlipAIClient = Substitute.For<IBlipAIClient>();
            sourceBlipAIClient.GetAllIntents(Arg.Any<bool>()).Returns(Task.FromResult<List<Intention>>(null));
            sourceBlipAIClient.GetAllEntities(Arg.Any<bool>()).Returns(Task.FromResult<List<Entity>>(null));

            var targetBlipAIClient = Substitute.For<IBlipAIClient>();
            targetBlipAIClient.GetAllIntents(Arg.Any<bool>()).Returns(Task.FromResult<List<Intention>>(null));
            targetBlipAIClient.GetAllEntities(Arg.Any<bool>()).Returns(Task.FromResult<List<Entity>>(null));

            var blipAIClientFactory = Substitute.For<IBlipClientFactory>();
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(fromKey))).Returns(sourceBlipAIClient);
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(toKey))).Returns(targetBlipAIClient);


            var handler = new CopyHandler(blipAIClientFactory);
            handler.FromAuthorization = new MyNamedParameter<string> { Value = fromKey };
            handler.ToAuthorization = new MyNamedParameter<string> { Value = toKey };
            handler.From = new MyNamedParameter<string>();
            handler.To = new MyNamedParameter<string>();
            handler.Verbose = new MySwitch { IsSet = false };
            handler.Force = new MySwitch { IsSet = false };
            handler.Contents = new MyNamedParameter<List<BucketNamespace>> { Value = new List<BucketNamespace> { BucketNamespace.AIModel } };

            //Act
            var result = handler.RunAsync(null).Result;

            //Assert
            Assert.AreEqual(0, result);

        }

        [Test]
        public void Copy_EmptyModel_To_Other_EmptyModel_WithForce()
        {
            //Arrange
            var fromKey = "key1";
            var toKey = "key2";

            var sourceBlipAIClient = Substitute.For<IBlipAIClient>();
            sourceBlipAIClient.GetAllIntents(Arg.Any<bool>()).Returns(Task.FromResult<List<Intention>>(null));
            sourceBlipAIClient.GetAllEntities(Arg.Any<bool>()).Returns(Task.FromResult<List<Entity>>(null));

            var targetBlipAIClient = Substitute.For<IBlipAIClient>();
            targetBlipAIClient.GetAllIntents(Arg.Any<bool>()).Returns(Task.FromResult<List<Intention>>(null));
            targetBlipAIClient.GetAllEntities(Arg.Any<bool>()).Returns(Task.FromResult<List<Entity>>(null));

            var blipAIClientFactory = Substitute.For<IBlipClientFactory>();
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(fromKey))).Returns(sourceBlipAIClient);
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(toKey))).Returns(targetBlipAIClient);


            var handler = new CopyHandler(blipAIClientFactory);
            handler.FromAuthorization = new MyNamedParameter<string> { Value = fromKey };
            handler.ToAuthorization = new MyNamedParameter<string> { Value = toKey };
            handler.From = new MyNamedParameter<string>();
            handler.To = new MyNamedParameter<string>();
            handler.Verbose = new MySwitch { IsSet = false };
            handler.Force = new MySwitch { IsSet = true };
            handler.Contents = new MyNamedParameter<List<BucketNamespace>> { Value = new List<BucketNamespace> { BucketNamespace.AIModel } };

            //Act
            var result = handler.RunAsync(null).Result;

            //Assert
            Assert.AreEqual(0, result);

        }

        [TestCase(0, 0)]
        [TestCase(0, 1)]
        [TestCase(0, 5)]
        [TestCase(0, 100)]
        [TestCase(1, 1)]
        [TestCase(1, 5)]
        [TestCase(1, 100)]
        [TestCase(5, 0)]
        [TestCase(5, 1)]
        [TestCase(5, 5)]
        [TestCase(5, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 1)]
        [TestCase(100, 5)]
        [TestCase(100, 100)]
        public void Copy_Model_To_EmptyModel_WithoutForce(int numIntents, int numEntities)
        {
            //Arrange
            var fromKey = "key1";
            var toKey = "key2";

            var sourceBlipAIClient = Substitute.For<IBlipAIClient>();
            var intents = new List<Intention>();
            for (int i = 0; i < numIntents; i++)
            {
                intents.Add(new Intention { Id = $"{i}", Name = $"Name{i}" });
            }
            var entities = new List<Entity>();
            for (int i = 0; i < numEntities; i++)
            {
                entities.Add(new Entity { Id = $"{i}", Name = $"Name{i}" });
            }
            sourceBlipAIClient.GetAllIntents(Arg.Any<bool>()).Returns(Task.FromResult(intents));
            sourceBlipAIClient.GetAllEntities(Arg.Any<bool>()).Returns(Task.FromResult(entities));

            var targetBlipAIClient = Substitute.For<IBlipAIClient>();
            targetBlipAIClient.GetAllIntents(Arg.Any<bool>()).Returns(Task.FromResult<List<Intention>>(null));
            targetBlipAIClient.GetAllEntities(Arg.Any<bool>()).Returns(Task.FromResult<List<Entity>>(null));

            var blipAIClientFactory = Substitute.For<IBlipClientFactory>();
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(fromKey))).Returns(sourceBlipAIClient);
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(toKey))).Returns(targetBlipAIClient);


            var handler = new CopyHandler(blipAIClientFactory);
            handler.FromAuthorization = new MyNamedParameter<string> { Value = fromKey };
            handler.ToAuthorization = new MyNamedParameter<string> { Value = toKey };
            handler.From = new MyNamedParameter<string>();
            handler.To = new MyNamedParameter<string>();
            handler.Verbose = new MySwitch { IsSet = false };
            handler.Force = new MySwitch { IsSet = false };
            handler.Contents = new MyNamedParameter<List<BucketNamespace>> { Value = new List<BucketNamespace> { BucketNamespace.AIModel } };

            //Act
            var result = handler.RunAsync(null).Result;

            //Assert
            Assert.AreEqual(0, result);
            targetBlipAIClient.Received(numEntities).AddEntity(Arg.Any<Entity>());
            targetBlipAIClient.Received(numIntents).AddIntent(Arg.Any<string>());
            targetBlipAIClient.DidNotReceive().DeleteIntent(Arg.Any<string>());
            targetBlipAIClient.DidNotReceive().DeleteEntity(Arg.Any<string>());
        }

        [TestCase(0, 0)]
        [TestCase(0, 1)]
        [TestCase(0, 5)]
        [TestCase(0, 100)]
        [TestCase(1, 1)]
        [TestCase(1, 5)]
        [TestCase(1, 100)]
        [TestCase(5, 0)]
        [TestCase(5, 1)]
        [TestCase(5, 5)]
        [TestCase(5, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 1)]
        [TestCase(100, 5)]
        [TestCase(100, 100)]
        public void Copy_Model_To_EmptyModel_WithForce(int numIntents, int numEntities)
        {
            //Arrange
            var fromKey = "key1";
            var toKey = "key2";

            var sourceBlipAIClient = Substitute.For<IBlipAIClient>();
            var intents = new List<Intention>();
            for (int i = 0; i < numIntents; i++)
            {
                intents.Add(new Intention { Id = $"{i}", Name = $"Name{i}" });
            }
            var entities = new List<Entity>();
            for (int i = 0; i < numEntities; i++)
            {
                entities.Add(new Entity { Id = $"{i}", Name = $"Name{i}" });
            }
            sourceBlipAIClient.GetAllIntents(Arg.Any<bool>()).Returns(Task.FromResult(intents));
            sourceBlipAIClient.GetAllEntities(Arg.Any<bool>()).Returns(Task.FromResult(entities));

            var targetBlipAIClient = Substitute.For<IBlipAIClient>();
            targetBlipAIClient.GetAllIntents(Arg.Any<bool>()).Returns(Task.FromResult<List<Intention>>(null));
            targetBlipAIClient.GetAllEntities(Arg.Any<bool>()).Returns(Task.FromResult<List<Entity>>(null));

            var blipAIClientFactory = Substitute.For<IBlipClientFactory>();
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(fromKey))).Returns(sourceBlipAIClient);
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(toKey))).Returns(targetBlipAIClient);


            var handler = new CopyHandler(blipAIClientFactory);
            handler.FromAuthorization = new MyNamedParameter<string> { Value = fromKey };
            handler.ToAuthorization = new MyNamedParameter<string> { Value = toKey };
            handler.From = new MyNamedParameter<string>();
            handler.To = new MyNamedParameter<string>();
            handler.Verbose = new MySwitch { IsSet = false };
            handler.Force = new MySwitch { IsSet = true };
            handler.Contents = new MyNamedParameter<List<BucketNamespace>> { Value = new List<BucketNamespace> { BucketNamespace.AIModel } };

            //Act
            var result = handler.RunAsync(null).Result;

            //Assert
            Assert.AreEqual(0, result);
            targetBlipAIClient.Received(numEntities).AddEntity(Arg.Any<Entity>());
            targetBlipAIClient.Received(numIntents).AddIntent(Arg.Any<string>());
            targetBlipAIClient.DidNotReceive().DeleteIntent(Arg.Any<string>());
            targetBlipAIClient.DidNotReceive().DeleteEntity(Arg.Any<string>());
        }

        [TestCase(0, 0)]
        [TestCase(0, 1)]
        [TestCase(0, 5)]
        [TestCase(0, 100)]
        [TestCase(1, 1)]
        [TestCase(1, 5)]
        [TestCase(1, 100)]
        [TestCase(5, 0)]
        [TestCase(5, 1)]
        [TestCase(5, 5)]
        [TestCase(5, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 1)]
        [TestCase(100, 5)]
        [TestCase(100, 100)]
        public void Copy_EmptyModel_To_Model_WithForce(int numIntents, int numEntities)
        {
            //Arrange
            var fromKey = "key1";
            var toKey = "key2";

            
            var intents = new List<Intention>();
            for (int i = 0; i < numIntents; i++)
            {
                intents.Add(new Intention { Id = $"{i}", Name = $"Name{i}" });
            }
            var entities = new List<Entity>();
            for (int i = 0; i < numEntities; i++)
            {
                entities.Add(new Entity { Id = $"{i}", Name = $"Name{i}" });
            }

            var sourceBlipAIClient = Substitute.For<IBlipAIClient>();
            sourceBlipAIClient.GetAllIntents(Arg.Any<bool>()).Returns(Task.FromResult<List<Intention>>(null));
            sourceBlipAIClient.GetAllEntities(Arg.Any<bool>()).Returns(Task.FromResult<List<Entity>>(null));

            var targetBlipAIClient = Substitute.For<IBlipAIClient>();
            targetBlipAIClient.GetAllIntents(Arg.Any<bool>(), Arg.Any<bool>()).Returns(Task.FromResult(intents));
            targetBlipAIClient.GetAllEntities(Arg.Any<bool>()).Returns(Task.FromResult(entities));

            var blipAIClientFactory = Substitute.For<IBlipClientFactory>();
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(fromKey))).Returns(sourceBlipAIClient);
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(toKey))).Returns(targetBlipAIClient);


            var handler = new CopyHandler(blipAIClientFactory);
            handler.FromAuthorization = new MyNamedParameter<string> { Value = fromKey };
            handler.ToAuthorization = new MyNamedParameter<string> { Value = toKey };
            handler.From = new MyNamedParameter<string>();
            handler.To = new MyNamedParameter<string>();
            handler.Verbose = new MySwitch { IsSet = false };
            handler.Force = new MySwitch { IsSet = true };
            handler.Contents = new MyNamedParameter<List<BucketNamespace>> { Value = new List<BucketNamespace> { BucketNamespace.AIModel } };

            //Act
            var result = handler.RunAsync(null).Result;

            //Assert
            Assert.AreEqual(0, result);
            targetBlipAIClient.DidNotReceive().AddEntity(Arg.Any<Entity>());
            targetBlipAIClient.DidNotReceive().AddIntent(Arg.Any<string>());
            targetBlipAIClient.Received(numIntents).DeleteIntent(Arg.Any<string>());
            targetBlipAIClient.Received(numEntities).DeleteEntity(Arg.Any<string>());
        }


        [TestCase(0, 0)]
        [TestCase(0, 1)]
        [TestCase(0, 5)]
        [TestCase(0, 100)]
        [TestCase(1, 1)]
        [TestCase(1, 5)]
        [TestCase(1, 100)]
        [TestCase(5, 0)]
        [TestCase(5, 1)]
        [TestCase(5, 5)]
        [TestCase(5, 100)]
        [TestCase(100, 0)]
        [TestCase(100, 1)]
        [TestCase(100, 5)]
        [TestCase(100, 100)]
        public void Copy_EmptyModel_To_Model_WithoutForce(int numIntents, int numEntities)
        {
            //Arrange
            var fromKey = "key1";
            var toKey = "key2";


            var intents = new List<Intention>();
            for (int i = 0; i < numIntents; i++)
            {
                intents.Add(new Intention { Id = $"{i}", Name = $"Name{i}" });
            }
            var entities = new List<Entity>();
            for (int i = 0; i < numEntities; i++)
            {
                entities.Add(new Entity { Id = $"{i}", Name = $"Name{i}" });
            }

            var sourceBlipAIClient = Substitute.For<IBlipAIClient>();
            sourceBlipAIClient.GetAllIntents(Arg.Any<bool>()).Returns(Task.FromResult<List<Intention>>(null));
            sourceBlipAIClient.GetAllEntities(Arg.Any<bool>()).Returns(Task.FromResult<List<Entity>>(null));

            var targetBlipAIClient = Substitute.For<IBlipAIClient>();
            targetBlipAIClient.GetAllIntents(Arg.Any<bool>()).Returns(Task.FromResult(intents));
            targetBlipAIClient.GetAllEntities(Arg.Any<bool>()).Returns(Task.FromResult(entities));

            var blipAIClientFactory = Substitute.For<IBlipClientFactory>();
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(fromKey))).Returns(sourceBlipAIClient);
            blipAIClientFactory.GetInstanceForAI(Arg.Is<string>(s => s.Equals(toKey))).Returns(targetBlipAIClient);


            var handler = new CopyHandler(blipAIClientFactory);
            handler.FromAuthorization = new MyNamedParameter<string> { Value = fromKey };
            handler.ToAuthorization = new MyNamedParameter<string> { Value = toKey };
            handler.From = new MyNamedParameter<string>();
            handler.To = new MyNamedParameter<string>();
            handler.Verbose = new MySwitch { IsSet = false };
            handler.Force = new MySwitch { IsSet = false };
            handler.Contents = new MyNamedParameter<List<BucketNamespace>> { Value = new List<BucketNamespace> { BucketNamespace.AIModel } };

            //Act
            var result = handler.RunAsync(null).Result;

            //Assert
            Assert.AreEqual(0, result);
            targetBlipAIClient.DidNotReceive().AddEntity(Arg.Any<Entity>());
            targetBlipAIClient.DidNotReceive().AddIntent(Arg.Any<string>());
            targetBlipAIClient.DidNotReceive().DeleteIntent(Arg.Any<string>());
            targetBlipAIClient.DidNotReceive().DeleteEntity(Arg.Any<string>());
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
