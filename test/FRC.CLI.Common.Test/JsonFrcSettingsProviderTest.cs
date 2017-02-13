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
        public async Task TestReadSettingsDoesNotExist()
        {
            using (var mock = AutoMock.GetLoose())
            {

                
                mock.Mock<IProjectInformationProvider>().Setup(x => x.GetProjectRootDirectoryAsync()).Returns(() => {
                    return Task.FromResult(Path.GetTempPath());
                });

                mock.Mock<IExceptionThrowerProvider>().Setup(x => x.ThrowException(It.IsAny<string>())).Returns(new Exception());

                if (File.Exists(jsonFile))
                {
                    try 
                    {
                        File.Delete(jsonFile);
                    }
                    catch (Exception)
                    {

                    }
                }


                var sut = mock.Create<JsonFrcSettingsProvider>();

                var actual = await sut.GetFrcSettingsAsync().ConfigureAwait(false);

                Assert.Null(actual);
            }
        }

        [Fact]
        public async Task TestReadSettingsInvalidJson1()
        {
            using (var mock = AutoMock.GetLoose())
            {

                
                mock.Mock<IProjectInformationProvider>().Setup(x => x.GetProjectRootDirectoryAsync()).Returns(() => {
                    return Task.FromResult(Path.GetTempPath());
                });

                mock.Mock<IExceptionThrowerProvider>().Setup(x => x.ThrowException(It.IsAny<string>())).Returns(new Exception());

                if (File.Exists(jsonFile))
                {
                    try 
                    {
                        File.Delete(jsonFile);
                    }
                    catch (Exception)
                    {

                    }
                }
                File.WriteAllText(jsonFile, @"
{
    tean: 9999
}                    
");
      

                var sut = mock.Create<JsonFrcSettingsProvider>();

                var actual = await sut.GetFrcSettingsAsync().ConfigureAwait(false);

                Assert.Null(actual);
            }
        }

        [Fact]
        public async Task TestReadSettingsLockedFile()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IProjectInformationProvider>().Setup(x => x.GetProjectRootDirectoryAsync()).Returns(() => {
                    return Task.FromResult(Path.GetTempPath());
                });

                mock.Mock<IExceptionThrowerProvider>().Setup(x => x.ThrowException(It.IsAny<string>())).Returns(new Exception());

                if (File.Exists(jsonFile))
                {
                    try 
                    {
                        File.Delete(jsonFile);
                    }
                    catch (Exception)
                    {

                    }
                }
                using (FileStream f = File.Create(jsonFile))
                {
                    var sut = mock.Create<JsonFrcSettingsProvider>();
                    var actual = await sut.GetFrcSettingsAsync().ConfigureAwait(false);
                    Assert.Null(actual);
                    
                }
            }
        }

        [Fact]
        public async Task TestReadSettingsOnlyTeamNumber()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IProjectInformationProvider>().Setup(x => x.GetProjectRootDirectoryAsync()).Returns(() => {
                    return Task.FromResult(Path.GetTempPath());
                });

                mock.Mock<IExceptionThrowerProvider>().Setup(x => x.ThrowException(It.IsAny<string>())).Returns(new Exception());

                if (File.Exists(jsonFile))
                {
                    try 
                    {
                        File.Delete(jsonFile);
                    }
                    catch (Exception)
                    {

                    }
                }

                File.WriteAllText(jsonFile, @"
{
  ""TeamNumber"": ""9999"",
  ""CommandLineArguments"": [],
  ""DeployIgnoreFiles"": []
}                  
");
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
