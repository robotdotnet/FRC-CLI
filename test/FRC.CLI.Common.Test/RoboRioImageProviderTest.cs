/*
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
using FRC.CLI.Base.Enums;
using Renci.SshNet;

namespace FRC.CLI.Common.Test
{
    public class RoboRioImageProviderTest
    {
        [Fact]
        public async Task TestGetAllowedImages()
        {
            using (var mock = AutoMock.GetStrict())
            {
                var testData = new List<string>
                {
                    "HelloWorld!",
                    "FRC Rocks!"
                };
                mock.Mock<IWPILibImageSettingsProvider>().Setup(x => 
                    x.GetAllowedImageVersionsAsync()).ReturnsAsync(testData.ToList());

                mock.Mock<IFileDeployerProvider>().Setup(x => x.Dispose());

                var sut = mock.Create<RoboRioImageProvider>();

                var actual = await sut.GetAllowedRoboRioImagesAsync().ConfigureAwait(false);

                Assert.NotNull(actual);
                Assert.Equal(testData, actual);
            }
        }

        [Fact]
        public async Task TestGetCurrentImageXmlValid()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<IFileDeployerProvider>().Setup(x => x.Dispose());

                mock.Mock<IFileDeployerProvider>().Setup(x => x.GetConnectionIpAsync()).ReturnsAsync(IPAddress.Loopback);

                mock.Mock<IFileDeployerProvider>().Setup(x => x.RunCommandAsync(It.IsAny<string>(), It.IsAny<ConnectionUser>()))
                    .ReturnsAsync<SshCommand>(new SshCommand() );

                var sut = mock.Create<RoboRioImageProvider>();

                var actual = await sut.GetCurrentRoboRioImageAsync().ConfigureAwait(false);

                Assert.NotNull(actual);
                Assert.Equal("FRC_roboRIO_2017_v8", actual);
            }
        }

        [Fact]
        public async Task TestCheckCurrentImageGoodImage()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<IFileDeployerProvider>().Setup(x => x.Dispose());

                mock.Mock<IFileDeployerProvider>().Setup(x => x.GetConnectionIpAsync()).ReturnsAsync(IPAddress.Loopback);

                var testData = new List<string>
                {
                    "FRC_roboRIO_2017_v8"
                };
                mock.Mock<IWPILibImageSettingsProvider>().Setup(x => 
                    x.GetAllowedImageVersionsAsync()).ReturnsAsync(testData.ToList());

                var sut = mock.Create<RoboRioImageProvider>();

                await sut.CheckCorrectImageAsync().ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task TestCheckCurrentImageBadImage()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<IFileDeployerProvider>().Setup(x => x.Dispose());

                mock.Mock<IFileDeployerProvider>().Setup(x => x.GetConnectionIpAsync()).ReturnsAsync(IPAddress.Loopback);

                var testData = new List<string>
                {
                    "FRC_roboRIO_2016_v19"
                };
                mock.Mock<IWPILibImageSettingsProvider>().Setup(x => 
                    x.GetAllowedImageVersionsAsync()).ReturnsAsync(testData.ToList());

                mock.Mock<IExceptionThrowerProvider>().Setup(x => x.ThrowException(It.IsAny<string>()))
                    .Returns<string>((s) => 
                    {
                        return new TestException(s);
                    });

                var sut = mock.Create<RoboRioImageProvider>();

                var exception = await Assert.ThrowsAsync<TestException>(async() =>
                {
                    await sut.CheckCorrectImageAsync().ConfigureAwait(false);
                });

                Assert.Contains("Current Version: FRC_roboRIO_2017_v8", exception.Msg);

                foreach(var item in testData)
                {
                    Assert.Contains($"    {item}", exception.Msg);
                    Assert.DoesNotContain($"    FRC_roboRIO_2017_v8", exception.Msg);
                }
            }
        }
    }
}
*/