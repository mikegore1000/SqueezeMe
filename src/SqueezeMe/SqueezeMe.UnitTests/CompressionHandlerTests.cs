using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using NUnit.Framework;
using System.Threading.Tasks;

namespace SqueezeMe.UnitTests
{
    [TestFixture]
    public class CompressionHandlerTests
    {
        [Datapoints] 
        public string[] Compressors = { "gzip", "deflate" };

        [Theory]
        public async void Given_A_Json_Payload_And_A_Single_Accept_Encoding_When_Requesting(string encoding)
        {
            var request = new HttpRequestMessage();
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(encoding));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new ObjectContent<string>("Request", new JsonMediaTypeFormatter());

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ObjectContent<string>("Response", new JsonMediaTypeFormatter());

            var testHandler = new TestHandler(response);
            var subject = new CompressionHandler { InnerHandler = testHandler };

            var invoker = new HttpMessageInvoker(subject, false);
            var result = await invoker.SendAsync(request, CancellationToken.None);

            Assert.That(result.Content.Headers.ContentEncoding, Contains.Item(encoding));
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Content, Is.TypeOf<CompressedContent>());
            Assert.That(result.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
        }

        [Theory]
        public async void Given_A_Json_Payload_And_Multiple_Accept_Encodings_When_Requesting()
        {
            var request = new HttpRequestMessage();
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate", 0.5));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip", 1));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new ObjectContent<string>("Request", new JsonMediaTypeFormatter());

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ObjectContent<string>("Response", new JsonMediaTypeFormatter());

            var testHandler = new TestHandler(response);
            var subject = new CompressionHandler { InnerHandler = testHandler };

            var invoker = new HttpMessageInvoker(subject, false);
            var result = await invoker.SendAsync(request, CancellationToken.None);

            Assert.That(result.Content.Headers.ContentEncoding, Contains.Item("gzip"));
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Content, Is.TypeOf<CompressedContent>());
            Assert.That(result.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
        }

        [Test]
        public async void Given_A_Json_Payload_With_No_Accept_Encoding_When_Requesting()
        {
            var request = new HttpRequestMessage();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new ObjectContent<string>("Request", new JsonMediaTypeFormatter());

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ObjectContent<string>("Response", new JsonMediaTypeFormatter());

            var testHandler = new TestHandler(response);
            var subject = new CompressionHandler { InnerHandler = testHandler };

            var invoker = new HttpMessageInvoker(subject, false);
            var result = await invoker.SendAsync(request, CancellationToken.None);

            Assert.That(result.Content.Headers.ContentEncoding, Is.Empty);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Content, Is.Not.TypeOf<CompressedContent>());
            Assert.That(result.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
        }

        [Theory]
        public async void Given_An_Empty_Payload_No_Compression_Is_Attempted(string encoding)
        {
            var request = new HttpRequestMessage();
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(encoding));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = new HttpResponseMessage(HttpStatusCode.OK);

            var testHandler = new TestHandler(response);
            var subject = new CompressionHandler { InnerHandler = testHandler };

            var invoker = new HttpMessageInvoker(subject, false);
            var result = await invoker.SendAsync(request, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.Not.TypeOf<CompressedContent>());
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        private class TestHandler : DelegatingHandler
        {
            private readonly HttpResponseMessage responseMessage;

            public TestHandler(HttpResponseMessage responseMessage)
            {
                this.responseMessage = responseMessage;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(responseMessage);
            }
        }
    }
}
