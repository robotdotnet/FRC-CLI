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


namespace FRC.CLI.Common.Test
{
    public class RobotCodeDeploymentProviderTest
    {
        List<string> FileListToDeploy = new List<string>()
        {
            @"C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libcscore.so",
            @"C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libHALAthena.so",
            @"C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libntcore.so",
            @"C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libOpenCvSharpExtern.so",
            @"C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_calib3d.so.3.1",
            @"C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_core.so.3.1",
            @"C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_features2d.so.3.1",
            @"C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_flann.so.3.1",
            @"C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_highgui.so.3.1",
            @"C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_imgcodecs.so.3.1",
            @"C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\MyRobot.exe",
            @"C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\MyRobot.pdb",
            @"C:\Users\redacted\Documents\VSTests\src\Robot451\bin\frctemp\wpinative\libopencv_imgcodecs.so.3.1",
        };

        [Fact]
        public void TestGetListOfFilesToDeployNoSettings()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<IFileDeployerProvider>().Setup(x => x.Dispose());

                mock.Create<RobotCodeDeploymentProvider>();


            }
        }
    }
}