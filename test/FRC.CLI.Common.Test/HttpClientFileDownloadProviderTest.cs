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
using System.Threading;
using System.Text;

namespace FRC.CLI.Common.Test
{
    public class MockHttpContent : HttpContent
    {
        public byte[] Content { get; set; }

        public MockHttpContent(string content)
        {
            Content = Encoding.UTF8.GetBytes(content);
        }

        protected async override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            await stream.WriteAsync(Content, 0, Content.Length);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = Content.Length;
            return true;
        }
    }
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        private HttpResponseMessage m_response;

        public MockHttpMessageHandler(HttpResponseMessage response)
        {
            m_response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(m_response);
        }
    }

    public class HttpClientFileDownloadProviderTest
    {
        [Fact]
        public async Task TestDownloadFileToStreamGoodResponse()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<IOutputWriter>().Setup(x => x.WriteLineAsync(It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

                var sut = mock.Create<HttpClientFileDownloadProvider>();
                string expected = 
@"Hello World!
Lets Check Text!
";
                var res = new HttpResponseMessage()
                {
                    Content = new MockHttpContent(expected)
                };
                sut.MessageHandler = new MockHttpMessageHandler(res);
                MemoryStream memStream = new MemoryStream();
                await sut.DownloadFileToStreamAsync("http://localhost:9090/test", memStream);
                memStream.Flush();
                memStream.Position = 0;
                StreamReader sr = new StreamReader(memStream, Encoding.UTF8);

                string result = sr.ReadToEnd();

                Assert.Equal(expected, result);
            }
        }

        [Fact]
        public async Task TestDownloadFileToStreamCannotConnect()
        {
            using (var mock = AutoMock.GetStrict())
            {
                mock.Mock<IOutputWriter>().Setup(x => x.WriteLineAsync(It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

                var sut = mock.Create<HttpClientFileDownloadProvider>();
                MemoryStream memStream = new MemoryStream();
                var ex = await Assert.ThrowsAsync<HttpRequestException>(async () => await sut.DownloadFileToStreamAsync("http://localhost:9090/test", memStream));
            }
        }

        [Fact]
        public async Task TestDownloadFileToStreamNonWritableStream()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IOutputWriter>().Setup(x => x.WriteLineAsync(It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

                var sut = mock.Create<HttpClientFileDownloadProvider>();
                string expected = 
@"Hello World!
Lets Check Text!
";
                var res = new HttpResponseMessage()
                {
                    Content = new MockHttpContent(expected)
                };
                var mockStream = new Mock<Stream>();
                mockStream.Setup(s => s.CanWrite).Returns(false);
                sut.MessageHandler = new MockHttpMessageHandler(res);
                var ex = await Assert.ThrowsAsync<ObjectDisposedException>(async () => await sut.DownloadFileToStreamAsync("http://localhost:9090/test", mockStream.Object));
            }
        }

        [Fact]
        public async Task TestDownloadFileToStreamDisposedStream()
        {
            using (var mock = AutoMock.GetLoose())
            {
                mock.Mock<IOutputWriter>().Setup(x => x.WriteLineAsync(It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

                var sut = mock.Create<HttpClientFileDownloadProvider>();
                string expected = 
@"Hello World!
Lets Check Text!
";
                var res = new HttpResponseMessage()
                {
                    Content = new MockHttpContent(expected)
                };
                MemoryStream memStream = new MemoryStream();
                memStream.Dispose();
                sut.MessageHandler = new MockHttpMessageHandler(res);
                var ex = await Assert.ThrowsAsync<ObjectDisposedException>(async () => await sut.DownloadFileToStreamAsync("http://localhost:9090/test", memStream));
            }
        }
    }
}