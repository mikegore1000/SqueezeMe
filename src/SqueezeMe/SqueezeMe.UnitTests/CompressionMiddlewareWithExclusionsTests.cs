using System.Net;
using System.Threading;
using NUnit.Framework;
using Owin;

namespace SqueezeMe.UnitTests
{
    [TestFixture]
    public class CompressionMiddlewareWithExclusionsTests : CompressionMiddlewareTestBase
    {
        protected override void AddCompressionMiddleware(IAppBuilder appBuilder)
        {
            appBuilder.UseCompression(excludedMimeTypes: new [] { "application/xml" });
        }

        [Test]
        public async void Given_An_Excluded_MimeType_No_Compression_Is_Attempted()
        {
            var request = Builder.WithAcceptEncoding("gzip").WithAccept("application/xml").Get();
            var result = await HttpClient.SendAsync(request, CancellationToken.None);

            Assert.That(result.Content.Headers.ContentEncoding, Is.Empty);
            Assert.That(result.Content.Headers.ContentType.MediaType, Is.EqualTo("application/xml"));
            Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
    }
}