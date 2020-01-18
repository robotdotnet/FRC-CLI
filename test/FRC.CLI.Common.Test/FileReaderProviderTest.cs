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
    public class FileReaderProviderTest
    {
        public static IEnumerable<object[]> TestData = new object[][]
        {
new object[] {@"
" },
new object[]{
@" 
This is a test string
" }, 
new object[]{
@"{
    ""Hello JSON!"", ""MoreJSON""
}
" }
        };

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task TestFileReadExists(string testText)
        {
            string tempFile = Path.GetTempFileName();
            try 
            {

                File.WriteAllText(tempFile, testText);

                var sut = new FileReaderProvider();
                string result = await sut.ReadFileAsStringAsync(tempFile);

                Assert.Equal(testText, result);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task TestFileReadMissing()
        {
            string tempFile = "TEMPDATADOESNTEXISTS";
            var sut = new FileReaderProvider();
            var result = await Assert.
            ThrowsAnyAsync<IOException>(async () => await sut.ReadFileAsStringAsync(tempFile));
        }

        [Fact]
        public async Task TestFileLocked()
        {
            string tempFile = Path.GetTempFileName();
            try 
            {

                using FileStream fs = new FileStream(tempFile, FileMode.Open);
                var sut = new FileReaderProvider();
                var result = await Assert.ThrowsAsync<IOException>(async () => await sut.ReadFileAsStringAsync(tempFile));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }
    }
}