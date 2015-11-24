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
        [Test]
        [TestCase("gzip")]
        [TestCase("deflate")]
        public async void Given_A_Json_Payload_When_Requesting_CompressedContentEncoding(string encoding)
        {
            var request = new HttpRequestMessage();
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(encoding));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(@"application/json"));
            request.Content = new ObjectContent<string>("Request", new JsonMediaTypeFormatter());

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ObjectContent<string>("Response", new JsonMediaTypeFormatter());

            var testHandler = new TestHandler(response);
            var subject = new CompressionHandler { InnerHandler = testHandler };

            var invoker = new HttpMessageInvoker(subject, false);
            var result = await invoker.SendAsync(request, CancellationToken.None);

            Assert.That(result.Content.Headers.ContentEncoding, Contains.Item(encoding));
        }

        [Test]
        public async void Given_A_Json_Payload_When_Requesting_NoContentEncoding()
        {
            var request = new HttpRequestMessage();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(@"application/json"));
            request.Content = new ObjectContent<string>("Request", new JsonMediaTypeFormatter());

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ObjectContent<string>("Response", new JsonMediaTypeFormatter());

            var testHandler = new TestHandler(response);
            var subject = new CompressionHandler { InnerHandler = testHandler };

            var invoker = new HttpMessageInvoker(subject, false);
            var result = await invoker.SendAsync(request, CancellationToken.None);

            Assert.That(result.Content.Headers.ContentEncoding, Is.Empty);
        }

        [Test]
        public async void Given_An_Empty_Payload_No_Compression_Is_Attempted()
        {
            var request = new HttpRequestMessage();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(@"application/json"));

            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new ObjectContent<string>("Response", new JsonMediaTypeFormatter());

            var testHandler = new TestHandler(response);
            var subject = new CompressionHandler { InnerHandler = testHandler };

            var invoker = new HttpMessageInvoker(subject, false);
            var result = await invoker.SendAsync(request, CancellationToken.None);

            Assert.That(result.Content.Headers.ContentEncoding, Is.Empty);
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
