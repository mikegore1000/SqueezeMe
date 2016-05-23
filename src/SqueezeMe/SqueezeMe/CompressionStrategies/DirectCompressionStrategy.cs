using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace SqueezeMe.CompressionStrategies
{
    internal class DirectCompressionStrategy : CompressionStrategy
    {
        internal override async Task Compress(Func<IDictionary<string, object>, Task> next, OwinContext context, ICompressor compressor, Stream httpOutputStream)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var compressedStream = compressor.CreateStream(memoryStream))
                {
                    context.Response.Body = compressedStream;
                    await next.Invoke(context.Environment);
                }

                if (memoryStream.Length > 0)
                {
                    SetResponseHeaders(context, compressor, memoryStream);
                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(httpOutputStream, BufferSize);
                }
            }
        }
    }
}