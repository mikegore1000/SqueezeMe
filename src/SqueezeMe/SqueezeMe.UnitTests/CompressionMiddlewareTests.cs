using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using NUnit.Framework;
using Microsoft.Owin.Builder;
using System;
using Owin;
using System.Web.Http;

namespace SqueezeMe.UnitTests
{
    [TestFixture]
    public class CompressionMiddlewareTests
    {
        private HttpClient httpClient;
        private RequestBuilder builder;

        [Datapoints] 
        public string[] Compressors = { "gzip", "deflate" };

        [SetUp]
        public void SetUp()
        {
            builder = new RequestBuilder();

            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            var appBuilder = new AppBuilder();
            appBuilder.Use(typeof(CompressionMiddleware));
            appBuilder.UseWebApi(config);

            httpClient = new HttpClient(new OwinHttpMessageHandler(appBuilder.Build()))
            {
                BaseAddress = new Uri("http://localhost")
            };
        }

        [Theory]
        public async void Given_A_Json_Payload_And_A_Single_Accept_Encoding_When_Requesting_The_Content_Is_Encrypted(string encoding)
        {
            var request = builder.WithAcceptEncoding(encoding).Get();
            var result = await httpClient.SendAsync(request);

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Content.Headers.ContentEncoding, Contains.Item(encoding));
            Assert.That(result.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
        }

        [Test]
        public async void Given_A_Json_Payload_And_An_Unexpected_Accept_Encoding_When_Requesting()
        {
            var request = builder.WithAcceptEncoding("bob").Get();

            var result = await httpClient.SendAsync(request);

            Assert.That(result.Content.Headers.ContentEncoding, Is.Empty);
        }

        [Theory]
        public async void Given_A_Json_Payload_And_Multiple_Accept_Encodings_When_Requesting()
        {
            var request = builder.Get();
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate", 0.5));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip", 1));

            var result = await httpClient.SendAsync(request, CancellationToken.None);

            Assert.That(result.Content.Headers.ContentEncoding, Contains.Item("gzip"));
            Assert.That(result.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
        }

        [Test]
        public async void Given_A_Json_Payload_With_No_Accept_Encoding_When_Requesting()
        {
            var request = builder.Get();
            var result = await httpClient.SendAsync(request, CancellationToken.None);

            Assert.That(result.Content.Headers.ContentEncoding, Is.Empty);
            Assert.That(result.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
        }

        [Theory]
        public async void Given_An_Empty_Payload_No_Compression_Is_Attempted(string encoding)
        {
            var request = builder.WithAcceptEncoding(encoding).Get();

            var result = await httpClient.SendAsync(request, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        private class RequestBuilder
        {
            private readonly HttpRequestMessage request = new HttpRequestMessage();

            public RequestBuilder()
            {
                request.RequestUri = new Uri("http://localhost/test");

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new ObjectContent<string>("Request", new JsonMediaTypeFormatter());
                request.Method = HttpMethod.Get;
            }

            public RequestBuilder WithAcceptEncoding(string acceptEncoding)
            {
                request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue(acceptEncoding));
                return this;
            }

            public HttpRequestMessage Get()
            {
                return request;
            }
        }
    }
}
