using Owin;

namespace SqueezeMe
{
    public static class AppBuilderExtensions
    {
        public static void UseCompression(this IAppBuilder app)
        {
            app.Use(typeof(CompressionMiddleware));
        }
    }
}