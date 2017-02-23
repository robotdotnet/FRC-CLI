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
using System.Reflection.Metadata;
using Autofac;

namespace FRC.CLI.Common.Test
{
    public class MonoRuntimeProviderTest
    {
        [Fact]
        public async Task TestDownloadToFileAsync()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<IFileDeployerProvider>().Setup(x => x.Dispose());

                mock.Mock<IOutputWriter>().Setup(x => x.WriteLineAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

                string expected = 
@"Hello World!
Lets Check Text!
"; 
                mock.Mock<IFileDownloadProvider>().Setup(x => x.DownloadFileToStreamAsync(It.IsAny<string>(),
                    It.IsAny<Stream>())).Returns<string, Stream>((x, y) => {
                        StreamWriter writer = new StreamWriter(y);
                        writer.Write(expected);
                        writer.Flush();
                        return Task.CompletedTask;
                    });

                var sut = mock.Create<MonoRuntimeProvider>();

                string tempFile = Path.GetTempFileName();
                try
                {
                    await sut.DownloadToFileAsync("", tempFile);

                    var result = File.ReadAllText(tempFile);

                    Assert.Equal(expected, result);
                }
                finally
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public async Task TestDownloadToFileAsyncLockedFile()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<IFileDeployerProvider>().Setup(x => x.Dispose());

                mock.Mock<IOutputWriter>().Setup(x => x.WriteLineAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

                string expected = 
@"Hello World!
Lets Check Text!
"; 
                mock.Mock<IFileDownloadProvider>().Setup(x => x.DownloadFileToStreamAsync(It.IsAny<string>(),
                    It.IsAny<Stream>())).Returns<string, Stream>((x, y) => {
                        StreamWriter writer = new StreamWriter(y);
                        writer.Write(expected);
                        writer.Flush();
                        return Task.CompletedTask;
                    });

                var sut = mock.Create<MonoRuntimeProvider>();

                string tempFile = Path.GetTempFileName();
                try
                {
                    using (var writer = File.Open(tempFile, FileMode.Open))
                    {
                        var ex = Assert.ThrowsAsync<IOException>(async () =>
                            await sut.DownloadToFileAsync("", tempFile));
                    }
                    
                }
                finally
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public async Task TestGetMonoFolder()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<IFileDeployerProvider>().Setup(x => x.Dispose());
                string tempPath = Path.GetTempPath();
                mock.Mock<IWPILibUserFolderResolver>()
                    .Setup(x => x.GetWPILibUserFolderAsync())
                    .ReturnsAsync(tempPath);

                var sut = mock.Create<MonoRuntimeProvider>();

                var result = await sut.GetMonoFolderAsync();

                Assert.Equal(Path.Combine(tempPath, "mono"), result);
            }
        }

        [Fact]
        public async Task TestInstallRuntimeMain()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<IFileDeployerProvider>().Setup(x => x.Dispose());
                string tempPath = Path.GetTempPath();
                mock.Mock<IWPILibUserFolderResolver>()
                    .Setup(x => x.GetWPILibUserFolderAsync())
                    .ReturnsAsync(tempPath);
                int count = 0;
                string str = null;
                var rpip = mock.Mock<IRemotePackageInstallerProvider>().Object;
                var exp = mock.Mock<IExceptionThrowerProvider>().Object;
                var fdp = mock.Mock<IFileDeployerProvider>().Object;
                var wufr = mock.Mock<IWPILibUserFolderResolver>().Object;
                var fdlp = mock.Mock<IFileDownloadProvider>().Object;
                var md5 = mock.Mock<IMd5HashCheckerProvider>().Object;
                var ow = mock.Mock<IOutputWriter>().Object;
                var sut = new Mock<MonoRuntimeProvider>(
                    rpip, fdp, exp, wufr, fdlp, md5, ow
                );

                sut.CallBase = true;
                sut.Setup(x => x.InstallRuntimeAsync(It.IsAny<string>()))
                    .Callback<string>(s => {
                        count++;
                        str = s;
                    }).Returns(Task.CompletedTask);


                await sut.Object.InstallRuntimeAsync();

                sut.Verify(x => x.InstallRuntimeAsync(It.IsAny<string>()), Times.Once);
                Assert.Equal(1, count);
                Assert.NotNull(str);
                Assert.Equal(Path.Combine(tempPath, "mono", DeployProperties.MonoVersion), str);
            }
        }
    }
}