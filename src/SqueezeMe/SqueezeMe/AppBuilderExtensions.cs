using System.Collections.Generic;
using Owin;

namespace SqueezeMe
{
    public static class AppBuilderExtensions
    {
        public static void UseCompression(this IAppBuilder app)
        {
            UseCompression(app, null);
        }

        public static void UseCompression(this IAppBuilder app, IEnumerable<string> excludedMimeTypes)
        {
            app.Use(typeof(CompressionMiddleware), excludedMimeTypes);
        }
    }
}