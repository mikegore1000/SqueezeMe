using System.Collections.Generic;
using Owin;

namespace SqueezeMe
{
    public static class AppBuilderExtensions
    {
        public static IAppBuilder UseCompression(this IAppBuilder app)
        {
            UseCompression(app, null);

            return app;
        }

        public static IAppBuilder UseCompression(this IAppBuilder app, IEnumerable<string> excludedMimeTypes)
        {
            app.Use(typeof(CompressionMiddleware), excludedMimeTypes);

            return app;
        }
    }
}