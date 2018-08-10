using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Take.BlipCLI.Services;

namespace Take.Blip.CLI.Tests.Services
{
    [TestFixture]
    public class NLPAnalyseFileServiceTest
    {
        public string PathToFile { get; set; }

        [SetUp]
        public void SetUp()
        {
            PathToFile = @".\temp\test\";
            Directory.CreateDirectory(PathToFile);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete(PathToFile, recursive: true);
        }

        [Test]
        public async Task When_ReadFile_Should_Works()
        {
            //Arrange
            var fileReader = new NLPAnalyseFileService();
            
            var file = "file.txt";
            var collection = new List<string> { "a", "b", "c", "d", "e" };
            using (var writer = new StreamWriter(PathToFile + file))
            {
                foreach (var item in collection)
                {
                    await writer.WriteLineAsync(item);
                }
            }

            //Act
            var inputList = await fileReader.GetInputsToAnalyseAsync(PathToFile + file);

            //Assert
            for (int i = 0; i < collection.Count; i++)
            {
                Assert.AreEqual(collection[i], inputList[i]);
            }
            
        }
    }
}
