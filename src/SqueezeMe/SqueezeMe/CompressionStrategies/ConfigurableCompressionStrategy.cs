using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace SqueezeMe.CompressionStrategies
{
    internal class ConfigurableCompressionStrategy : CompressionStrategy
    {
        private readonly IEnumerable<string> excludedMimeTypes;

        internal ConfigurableCompressionStrategy(IEnumerable<string> excludedMimeTypes)
        {
            if (excludedMimeTypes == null)
            {
                throw new ArgumentNullException(nameof(excludedMimeTypes));
            }

            this.excludedMimeTypes = excludedMimeTypes;
        }

        internal override async Task Compress(Func<IDictionary<string, object>, Task> next, OwinContext context, ICompressor compressor, Stream httpOutputStream)
        {
            using (var uncompressedStream = new MemoryStream())
            {
                context.Response.Body = uncompressedStream;
                await next.Invoke(context.Environment);
                await uncompressedStream.FlushAsync();

                if (uncompressedStream.Length > 0)
                {
                    uncompressedStream.Position = 0;

                    if (ShouldCompress(context.Response.ContentType))
                    {
                        await CompressToHttpOutputStream(context, compressor, httpOutputStream, uncompressedStream);
                    }
                    else
                    {
                        await uncompressedStream.CopyToAsync(httpOutputStream, BufferSize);
                    }
                }
            }
        }

        private static async Task CompressToHttpOutputStream(OwinContext context, ICompressor compressor, Stream httpOutputStream, MemoryStream uncompressedStream)
        {
            using (var compressedStream = new MemoryStream())
            {
                using (var compressionStream = compressor.CreateStream(compressedStream))
                {
                    await uncompressedStream.CopyToAsync(compressionStream, BufferSize);
                }

                compressedStream.Position = 0;

                SetResponseHeaders(context, compressor, compressedStream);
                await compressedStream.CopyToAsync(httpOutputStream, BufferSize);
            }
        }

        private bool ShouldCompress(string responseMimeType)
        {
            return !excludedMimeTypes.Any(x => responseMimeType.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}