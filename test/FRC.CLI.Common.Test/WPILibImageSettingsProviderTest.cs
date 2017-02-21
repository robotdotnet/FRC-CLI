using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using FRC.CLI.Base.Interfaces;
using FRC.CLI.Common.Implementations;
using Moq;
using Xunit;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace FRC.CLI.Common.Test
{
    public class WPILibImageSettingsProviderTest
    {
        [Fact]
        public void TestGetCombinedFilePath()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<INativeContentDeploymentProvider>()
                    .Setup(x => x.NativeDirectory)
                    .Returns(NativeContentDeploymentProvider.NativeDirectoryFolder);

                var sut = mock.Create<WPILibImageSettingsProvider>();
                const string pathRoot = @"C:\CustomPath\Root";
                var result = sut.GetCombinedFilePath(pathRoot);
                Assert.StartsWith(pathRoot, result);
                Assert.Equal(Path.Combine(pathRoot,
                    NativeContentDeploymentProvider.NativeDirectoryFolder, 
                    WPILibImageSettingsProvider.JsonFileName), result);
            }
        }

        [Fact]
        public void TestParseStringToJsonValidJsonSingleImage()
        {
            string json = 
@"{
  ""AllowedImages"": [
    ""FRC_roboRIO_2017_v8""
  ]
}
";
            using (var mock = AutoMock.GetStrict())
            {
                var sut = mock.Create<WPILibImageSettingsProvider>();
                var result = sut.ParseStringToJson(json);
                Assert.True(result.Count() == 1);
                Assert.Contains("FRC_roboRIO_2017_v8", result);
            }
        }

        [Fact]
        public void TestParseStringToJsonValidJsonMultipleImages()
        {
            string json = 
@"{
  ""AllowedImages"": [
    ""FRC_roboRIO_2017_v8"",
    ""FRC_roboRIO_2017_v9""
  ]
}
";
            using (var mock = AutoMock.GetStrict())
            {
                var sut = mock.Create<WPILibImageSettingsProvider>();
                var result = sut.ParseStringToJson(json);
                Assert.True(result.Count() == 2);
                Assert.Contains("FRC_roboRIO_2017_v8", result);
                Assert.Contains("FRC_roboRIO_2017_v9", result);
            }
        }

        [Fact]
        public void TestParseStringToJsonInvalidJson1()
        {
            string json = 
@"{
  ""9999"": [
}
";
            using (var mock = AutoMock.GetStrict())
            {
                var sut = mock.Create<WPILibImageSettingsProvider>();
                var exception = Assert.Throws<JsonSerializationException>(() => sut.ParseStringToJson(json));
            }
        }

        [Fact]
        public void TestParseStringToJsonInvalidJson2()
        {
            string json = 
@"{
  ""9999"": [
    ]
}
";
            using (var mock = AutoMock.GetStrict())
            {
                var sut = mock.Create<WPILibImageSettingsProvider>();
                var exception = Assert.Throws<JsonSerializationException>(() => sut.ParseStringToJson(json));
            }
        }
        //TODO: Figure out how to make these cases fail properly.
        /*
        [Fact]
        public void TestParseStringToJsonInvalidJson3()
        {
            string json = 
@"{
}
";
            using (var mock = AutoMock.GetStrict())
            {
                var sut = mock.Create<WPILibImageSettingsProvider>();
                var exception = Assert.Throws<JsonSerializationException>(() => sut.ParseStringToJson(json));
            }
        }

        [Fact]
        public void TestParseStringToJsonInvalidJson4()
        {
            string json = 
@"
";
            using (var mock = AutoMock.GetStrict())
            {
                var sut = mock.Create<WPILibImageSettingsProvider>();
                var exception = Assert.Throws<JsonSerializationException>(() => sut.ParseStringToJson(json));
            }
        }
        */

        [Fact]
        public async Task TestGetAllowedImageVersionsAsyncValid()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<INativeContentDeploymentProvider>()
                    .Setup(x => x.NativeDirectory)
                    .Returns(NativeContentDeploymentProvider.NativeDirectoryFolder);

                mock.Mock<IProjectInformationProvider>().Setup(x => x.GetProjectBuildDirectoryAsync())
                    .ReturnsAsync("buildRoot");
                
                mock.Mock<IFileReaderProvider>().Setup(x => x.ReadFileAsStringAsync(It.IsAny<string>()))
                    .ReturnsAsync(
@"{
  ""AllowedImages"": [
    ""FRC_roboRIO_2017_v8""
  ]
}
");

                var sut = mock.Create<WPILibImageSettingsProvider>();

                var result = await sut.GetAllowedImageVersionsAsync();

                Assert.True(result.Count() == 1);
                Assert.Contains("FRC_roboRIO_2017_v8", result);
            }
        }

        [Fact]
        public async Task TestGetAllowedImageVersionsMissingFile()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<INativeContentDeploymentProvider>()
                    .Setup(x => x.NativeDirectory)
                    .Returns(NativeContentDeploymentProvider.NativeDirectoryFolder);

                mock.Mock<IProjectInformationProvider>().Setup(x => x.GetProjectBuildDirectoryAsync())
                    .ReturnsAsync("buildRoot");
                
                mock.Mock<IFileReaderProvider>().Setup(x => x.ReadFileAsStringAsync(It.IsAny<string>()))
                    .Throws(new IOException());

                mock.Mock<IExceptionThrowerProvider>().Setup(x => x.ThrowException(It.IsAny<string>()))
                    .Returns<string>((s) => new TestException(s));

                var sut = mock.Create<WPILibImageSettingsProvider>();

                var result = await Assert.ThrowsAsync<TestException>(async () => await sut.GetAllowedImageVersionsAsync());

                Assert.NotNull(result);
                Assert.NotNull(result.Msg);
            }
        }

        [Fact]
        public async Task TestGetAllowedImageVersionsBadJson()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<INativeContentDeploymentProvider>()
                    .Setup(x => x.NativeDirectory)
                    .Returns(NativeContentDeploymentProvider.NativeDirectoryFolder);

                mock.Mock<IProjectInformationProvider>().Setup(x => x.GetProjectBuildDirectoryAsync())
                    .ReturnsAsync("buildRoot");
                
                mock.Mock<IFileReaderProvider>().Setup(x => x.ReadFileAsStringAsync(It.IsAny<string>()))
                    .ReturnsAsync(
@"{
  ""999"": [
    ""FRC_roboRIO_2017_v8""
  ]
}
");

                mock.Mock<IExceptionThrowerProvider>().Setup(x => x.ThrowException(It.IsAny<string>()))
                    .Returns<string>((s) => new TestException(s));

                var sut = mock.Create<WPILibImageSettingsProvider>();

                var result = await Assert.ThrowsAsync<TestException>(async () => await sut.GetAllowedImageVersionsAsync());

                Assert.NotNull(result);
                Assert.NotNull(result.Msg);
            }
        }
    }
}