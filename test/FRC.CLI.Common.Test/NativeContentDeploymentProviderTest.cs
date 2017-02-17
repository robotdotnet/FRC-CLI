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
using FRC.CLI.Base.Enums;
using Renci.SshNet;

namespace FRC.CLI.Common.Test
{
    public class NativeContentDeploymentProviderTest
    {
        [Fact]
        public async Task TestGetFilesToUpdateListMatching()
        {
            using (var mock = AutoMock.GetStrict())
            {
                List<(string, string)> testVals = new List<(string, string)>
                {
                    ("Test", "5555"),
                    ("Another", "342"),
                    ("A Third", "334211")
                };

                // Force the second to actually be a copy
                var remoteFiles = testVals.ToList();
                var localFiles = testVals.ToList();

                mock.Mock<IFileDeployerProvider>().Setup(x => x.Dispose());

                var sut = mock.Create<NativeContentDeploymentProvider>();

                var toDeploy = sut.GetFilesToUpdate(remoteFiles, localFiles).ToList();

                Assert.True(toDeploy.Count == 0);//(testVals, toDeploy);
            }
        }

        [Fact]
        public void TestGetFilesToUpdateListExtraLocal()
        {
            using (var mock = AutoMock.GetStrict())
            {
                List<(string, string)> testVals = new List<(string, string)>
                {
                    ("Test", "5555"),
                    ("Another", "342")
                };

                // Force the second to actually be a copy
                var remoteFiles = testVals.ToList();
                var localFiles = testVals.ToList();

                var added = ("A Third", "334211");
                localFiles.Add(added);

                mock.Mock<IFileDeployerProvider>().Setup(x => x.Dispose());

                var sut = mock.Create<NativeContentDeploymentProvider>();

                var toDeploy = sut.GetFilesToUpdate(remoteFiles, localFiles).ToList();

                Assert.Equal(new (string, string)[] {added}, toDeploy);
            }
        }

        [Fact]
        public void TestGetFilesToUpdateListExtraRemote()
        {
            using (var mock = AutoMock.GetStrict())
            {
                List<(string, string)> testVals = new List<(string, string)>
                {
                    ("Test", "5555"),
                    ("Another", "342")
                };

                // Force the second to actually be a copy
                var remoteFiles = testVals.ToList();
                var localFiles = testVals.ToList();

                var added = ("A Third", "334211");
                remoteFiles.Add(added);

                mock.Mock<IFileDeployerProvider>().Setup(x => x.Dispose());

                var sut = mock.Create<NativeContentDeploymentProvider>();

                var toDeploy = sut.GetFilesToUpdate(remoteFiles, localFiles).ToList();

                Assert.True(toDeploy.Count == 0);//(testVals, toDeploy);
            }
        }

        [Fact]
        public void TestReadFilesFromStreamValidJson()
        {
            using (var mock = AutoMock.GetStrict())
            {
                List<(string, string)> testVals = new List<(string, string)>
                {
                    ("Test", "5555"),
                    ("Another", "342"),
                    ("A Third", "334211")
                };

                var json = JsonConvert.SerializeObject(testVals);
                MemoryStream memStream = new MemoryStream();
                {
                    StreamWriter writer = new StreamWriter(memStream);
                    writer.Write(json);
                    writer.Flush();
                }

                memStream.Position = 0;

                mock.Mock<IFileDeployerProvider>().Setup(x => x.Dispose());

                var sut = mock.Create<NativeContentDeploymentProvider>();

                var filesFromJson = sut.ReadFilesFromStream(memStream);

                Assert.Equal(testVals, filesFromJson);
            }
        }

        [Fact]
        public void TestReadFilesFromStreamInvalidJson()
        {
            using (var mock = AutoMock.GetStrict())
            {
                var json = 
@"{
    ""Test"", ""RAWR""
}";

                MemoryStream memStream = new MemoryStream();
                {
                    StreamWriter writer = new StreamWriter(memStream);
                    writer.Write(json);
                    writer.Flush();
                }

                memStream.Position = 0;


                mock.Mock<IFileDeployerProvider>().Setup(x => x.Dispose());

                var sut = mock.Create<NativeContentDeploymentProvider>();

                var filesFromJson = sut.ReadFilesFromStream(memStream).ToList(); 

                Assert.True(filesFromJson.Count == 0);//(testVals, toDeploy);
            }
        }

        [Fact]
        public void TestReadFilesFromWPILibJson()
        {
            string wpiLibProps = 
@"C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libcscore.so=171791279411514164571492124812924515682117
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libHALAthena.so=23415252104138815423210337237251299842213
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libntcore.so=5479587154230127160217123110752018785108
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libOpenCvSharpExtern.so=207347186175213132421841292211712210717918
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_calib3d.so.3.1=11831125621565872421151641441115424110126
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_core.so.3.1=1705172228249252239195832519399100154195
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_features2d.so.3.1=1330108181261616292222032393117529168169
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_flann.so.3.1=7517322166167114741306691756519324510536
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_highgui.so.3.1=11117656232241698144282329494982042091
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_imgcodecs.so.3.1=37113157920058451881892010197249746469
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_imgproc.so.3.1=2378121620212013716315227182752526330141
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_ml.so.3.1=21918515820436107152232841137488227239239
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_objdetect.so.3.1=19871133629902151231112934152253243126
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_photo.so.3.1=1801488086216235204592538582148648791252
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_shape.so.3.1=146111642328156024620184861071371342420
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_stitching.so.3.1=40482311269819399241239103112611171399987
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_superres.so.3.1=1819862196835129177207210223158205196240
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_video.so.3.1=11925040195691210144209659619816086162206
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_videoio.so.3.1=88183201255182413823222161238681501371439
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_videostab.so.3.1=17071931261128016793722311168836845176
C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libwpiutil.so=22182431212061718787981061221239560126102
";

            using (var mock = AutoMock.GetStrict())
            {
                MemoryStream memStream = new MemoryStream();
                {
                    StreamWriter writer = new StreamWriter(memStream);
                    writer.Write(wpiLibProps);
                    writer.Flush();
                }

                memStream.Position = 0;


                mock.Mock<IFileDeployerProvider>().Setup(x => x.Dispose());

                var sut = mock.Create<NativeContentDeploymentProvider>();

                var filesFromJson = sut.ReadFilesFromStream(memStream).ToList(); 

                Assert.True(filesFromJson.Count == 0);//(testVals, toDeploy);
            }
        }

        [Fact]
        public async Task TestDeployNativeFiles()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<IFileDeployerProvider>().Setup(x => x.Dispose());
                
                IEnumerable<string> filesToDeploy = null;
                string deployLoc = null;
                ConnectionUser conn = (ConnectionUser)5;

                var fProvider = mock.Mock<IFileDeployerProvider>().Setup(x => x.DeployFilesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), 
                    It.IsAny<ConnectionUser>())).Callback<IEnumerable<string>, string, ConnectionUser>((f, p, u) => {
                        filesToDeploy = f;
                        deployLoc = p;
                        conn = u;
                    }).ReturnsAsync(true);

                string command = null;
                ConnectionUser conn2 = (ConnectionUser)5;

                mock.Mock<IFileDeployerProvider>().Setup(x => x.RunCommandAsync(It.IsAny<string>(), 
                    It.IsAny<ConnectionUser>())).Callback<string, ConnectionUser>((p, u) => {
                        command = p;
                        conn2 = u;
                    }).ReturnsAsync(null);

                var m = mock.Mock<IWPILibNativeDeploySettingsProvider>().SetupGet(x => x.NativeDeployLocation).Returns("/usr/local/frc/lib");

                var sut = mock.Create<NativeContentDeploymentProvider>();

                List<string> depFiles = new List<string>
                {
                    "Hello",
                    "World",
                    "555",
                    "Timers"
                };

                await sut.DeployNativeFilesAsync(depFiles);

                mock.Mock<IFileDeployerProvider>().Verify(x => x.DeployFilesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), 
                    It.IsAny<ConnectionUser>()), Times.Once);

                mock.Mock<IFileDeployerProvider>().Verify(x => x.RunCommandAsync(It.IsAny<string>(), 
                    It.IsAny<ConnectionUser>()), Times.Once);

                Assert.Equal("ldconfig", command);
                Assert.Equal(depFiles, filesToDeploy);
                Assert.Equal(ConnectionUser.Admin, conn);
                Assert.Equal(ConnectionUser.Admin, conn2);
                Assert.Equal("/usr/local/frc/lib", deployLoc);
            }
        }
    }
}