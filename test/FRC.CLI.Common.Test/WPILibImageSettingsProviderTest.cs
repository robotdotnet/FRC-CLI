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
    public class WPILibImageSettingsProviderTest
    {
        private class TestException : Exception
        {
            public TestException(string msg) : base (msg)
            {

            }
        }

        [Fact]
        public async Task TestReadSettingsInvalidJson1()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<IProjectInformationProvider>().Setup(x => x.GetProjectBuildDirectoryAsync()).Returns(() => {
                    return Task.FromResult(Path.GetTempPath());
                });
                string badJson =
@"{
    tean: 9.9.99
}                    
";
                mock.Mock<INativeContentDeploymentProvider>().SetupGet(x => x.NativeDirectory).Returns("wpinative");
                mock.Mock<IExceptionThrowerProvider>().Setup(x => x.ThrowException(It.IsAny<string>())).Returns<string>(x => new TestException(x));

                mock.Mock<IFileReaderProvider>().Setup(x => x.ReadFileAsStringAsync(It.IsAny<string>())).ReturnsAsync(badJson);

                var sut = mock.Create<WPILibImageSettingsProvider>();
                
                var ex = await Assert.ThrowsAsync<TestException>(async () => await sut.GetAllowedImageVersionsAsync());
            }
        }

        [Fact]
        public async Task TestReadSettingsValidJson()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<IProjectInformationProvider>().Setup(x => x.GetProjectBuildDirectoryAsync()).Returns(() => {
                    return Task.FromResult(Path.GetTempPath());
                });

                mock.Mock<INativeContentDeploymentProvider>().SetupGet(x => x.NativeDirectory).Returns("wpinative");
                string goodJson =
@"{
  ""AllowedImages"": [
    ""FRC_roboRIO_2017_v8""
  ]
}                
";
                mock.Mock<IExceptionThrowerProvider>().Setup(x => x.ThrowException(It.IsAny<string>())).Returns<string>(x => new TestException(x));

                mock.Mock<IFileReaderProvider>().Setup(x => x.ReadFileAsStringAsync(It.IsAny<string>())).ReturnsAsync(goodJson);

                var sut = mock.Create<WPILibImageSettingsProvider>();

                var result = await sut.GetAllowedImageVersionsAsync();
                
                //var ex = await Assert.ThrowsAsync<TestException>(async () => await sut.GetAllowedImageVersionsAsync());
            }
        }
    }
}