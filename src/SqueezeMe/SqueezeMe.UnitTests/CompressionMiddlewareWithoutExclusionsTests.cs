using NUnit.Framework;
using Owin;

namespace SqueezeMe.UnitTests
{
    [TestFixture]
    public class CompressionMiddlewareWithoutExclusionsTests : CompressionMiddlewareTestBase
    {
        protected override void AddCompressionMiddleware(IAppBuilder appBuilder)
        {
            appBuilder.UseCompression(excludedMimeTypes: new[] { "application/xml" });
        }
    }
}