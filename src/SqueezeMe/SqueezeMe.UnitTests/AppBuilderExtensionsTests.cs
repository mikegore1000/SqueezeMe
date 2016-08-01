using Microsoft.Owin.Builder;
using NUnit.Framework;

namespace SqueezeMe.UnitTests
{
    [TestFixture]
    public class AppBuilderExtensionsTests
    {
        [Test]
        public void When_Compression_Is_Added_To_The_AppBuilder_The_AppBuilder_Is_Returned()
        {
            var builder = new AppBuilder();

            var returnedBuilder = builder.UseCompression();

            Assert.That(returnedBuilder, Is.EqualTo(builder));
        }

        [Test]
        public void When_Compression_Is_Added_To_The_AppBuilder_With_Exclusions_The_AppBuilder_Is_Returned()
        {
            var builder = new AppBuilder();

            var returnedBuilder = builder.UseCompression(new [] { @"application/json" });

            Assert.That(returnedBuilder, Is.EqualTo(builder));
        }
    }
}
