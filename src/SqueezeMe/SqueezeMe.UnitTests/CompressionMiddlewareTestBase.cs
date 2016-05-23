using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http;
using Microsoft.Owin.Builder;
using NUnit.Framework;
using Owin;

namespace SqueezeMe.UnitTests
{
    public abstract class CompressionMiddlewareTestBase
    {
        protected HttpClient HttpClient { get; private set; }

        protected RequestBuilder Builder { get; private set; }

        [Datapoints]
        public string[] Compressors = { "gzip", "deflate" };

        [SetUp]
        public void SetUp()
        {
            Builder = new RequestBuilder();

            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();

            var appBuilder = new AppBuilder();
            AddCompressionMiddleware(appBuilder);
            appBuilder.UseWebApi(config);

            HttpClient = new HttpClient(new OwinHttpMessageHandler(appBuilder.Build()))
            {
                BaseAddress = new Uri("http://localhost")
            };

        }

        protected abstract void AddCompressionMiddleware(IAppBuilder appBuilder);

        [Theory]
        public async void Given_A_Json_Payload_And_A_Single_Accept_Encoding_When_Requesting_The_Content_Is_Encrypted(string encoding)
        {
            var request = Builder.WithAcceptEncoding(encoding).Get();
            var result = await HttpClient.SendAsync(request);

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Content.Headers.ContentEncoding, Contains.Item(encoding));
            Assert.That(result.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
        }

        [Test]
        public async void Given_A_Json_Payload_And_An_Unexpected_Accept_Encoding_When_Requesting()
        {
            var request = Builder.WithAcceptEncoding("bob").Get();

            var result = await HttpClient.SendAsync(request);

            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result.Content.Headers.ContentEncoding, Is.Empty);
        }

        [Test]
        public async void Given_A_Json_Payload_And_Multiple_Accept_Encodings_When_Requesting_Then_The_Quality_Header_Is_Respected()
        {
            var request = Builder.Get();
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate", 0.5));
            request.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip", 1));

            var result = await HttpClient.SendAsync(request, CancellationToken.None);

            Assert.That(result.Content.Headers.ContentEncoding, Contains.Item("gzip"));
            Assert.That(result.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
        }

        [Test]
        public async void Given_A_Json_Payload_With_No_Accept_Encoding_No_Compression_Is_Attempted()
        {
            var request = Builder.Get();
            var result = await HttpClient.SendAsync(request, CancellationToken.None);

            Assert.That(result.Content.Headers.ContentEncoding, Is.Empty);
            Assert.That(result.Content.Headers.ContentType.MediaType, Is.EqualTo("application/json"));
        }

        [Theory]
        public async void Given_An_Empty_Payload_No_Compression_Is_Attempted(string encoding)
        {
            var request = Builder.WithAcceptEncoding(encoding).Get();

            var result = await HttpClient.SendAsync(request, CancellationToken.None);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}