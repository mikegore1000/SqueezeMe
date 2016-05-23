using Owin;
using System.Collections.Generic;

namespace SqueezeMe
{
    public static class AppBuilderExtensions
    {
        /// <param name="excludeMime">
        /// A list of MIME types to exclude from compression.  Pass null or omit to use
        /// the internal list (typically common image/audio/video types).
        /// In order to compress EVERYTHING, pass in an empty array.
        /// </param>
        public static void UseCompression(this IAppBuilder app, ICollection<string> excludeMime = null)
        {
            if (excludeMime == null) 
            {
                excludeMime = new[]
                {
                    "image/gif", "image/jpeg", "image/pjpeg", "image/png",
                    "video/x-ms-asf", "video/avi", "video/msvideo", "video/x-msvideo",
                    "video/mpeg", "video/x-mpeg", "video/x-motion-jpeg", "video/quicktime", "video/mp4",
                    "audio/mpeg", "audio/x-mpeg", "audio/mpeg3", "audio/mp3", "audio/x-realaudio",
                    "audio/aac", "audio/x-aac", "audio/aacp", "audio/flac"
                };
            }

            app.Use< CompressionMiddleware>(excludeMime);
        }
    }
}