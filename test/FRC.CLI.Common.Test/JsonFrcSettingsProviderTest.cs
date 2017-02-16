using System;
using System.IO;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using FRC.CLI.Base.Interfaces;
using FRC.CLI.Common.Implementations;
using Moq;
using Xunit;

namespace FRC.CLI.Common.Test
{
    public class JsonFrcSettingsProviderTest
    {
        public string jsonFile = Path.Combine(Path.GetTempPath(), JsonFrcSettingsProvider.SettingsJsonFileName);

        [Fact]
        public async Task TestReadSettingsCatchesIoException()
        {
            using (var mock = AutoMock.GetStrict())
            {

                
                mock.Mock<IProjectInformationProvider>().Setup(x => x.GetProjectRootDirectoryAsync()).Returns(() => {
                    return Task.FromResult(Path.GetTempPath());
                });

                mock.Mock<IExceptionThrowerProvider>().Setup(x => x.ThrowException(It.IsAny<string>())).Returns(new Exception());
                mock.Mock<IBuildSettingsProvider>().SetupGet(x => x.Verbose).Returns(false);
                mock.Mock<IFileReaderProvider>().Setup(x => x.ReadFileAsStringAsync(It.IsAny<string>())).Throws(new IOException("Failed to read file"));

                var sut = mock.Create<JsonFrcSettingsProvider>();

                var actual = await sut.GetFrcSettingsAsync().ConfigureAwait(false);

                Assert.Null(actual);
            }
        }

        [Fact]
        public async Task TestReadSettingsInvalidJson1()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<IProjectInformationProvider>().Setup(x => x.GetProjectRootDirectoryAsync()).Returns(() => {
                    return Task.FromResult(Path.GetTempPath());
                });

                string badJson = 
@"{
    tean: 9999
}                    
";
                mock.Mock<IBuildSettingsProvider>().SetupGet(x => x.Verbose).Returns(false);
                mock.Mock<IExceptionThrowerProvider>().Setup(x => x.ThrowException(It.IsAny<string>())).Returns(new Exception());

                mock.Mock<IFileReaderProvider>().Setup(x => x.ReadFileAsStringAsync(It.IsAny<string>())).ReturnsAsync(badJson);
      
                var sut = mock.Create<JsonFrcSettingsProvider>();

                var actual = await sut.GetFrcSettingsAsync().ConfigureAwait(false);

                Assert.Null(actual);
            }
        }

        [Fact]
        public async Task TestReadSettingsOnlyTeamNumber()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<IProjectInformationProvider>().Setup(x => x.GetProjectRootDirectoryAsync()).Returns(() => {
                    return Task.FromResult(Path.GetTempPath());
                });

                mock.Mock<IExceptionThrowerProvider>().Setup(x => x.ThrowException(It.IsAny<string>())).Returns(new Exception());

                mock.Mock<IBuildSettingsProvider>().SetupGet(x => x.Verbose).Returns(false);

                string goodJson = 
@"{
  ""TeamNumber"": ""9999"",
  ""CommandLineArguments"": [],
  ""DeployIgnoreFiles"": []
}                  
";

                mock.Mock<IFileReaderProvider>().Setup(x => x.ReadFileAsStringAsync(It.IsAny<string>())).ReturnsAsync(goodJson);
                var sut = mock.Create<JsonFrcSettingsProvider>();
                var actual = await sut.GetFrcSettingsAsync().ConfigureAwait(false);
                Assert.NotNull(actual);
                Assert.Equal(actual.TeamNumber, "9999");
                Assert.True(actual.CommandLineArguments.Count == 0);
                Assert.True(actual.DeployIgnoreFiles.Count == 0);
            }
        }
    }
}
