using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace SqueezeMe.CompressionStrategies
{
    internal abstract class CompressionStrategy
    {
        protected const int BufferSize = 8192;

        internal abstract Task Compress(Func<IDictionary<string, object>, Task> next, OwinContext context, ICompressor compressor, Stream httpOutputStream);

        protected static void SetResponseHeaders(OwinContext context, ICompressor compressor, MemoryStream memoryStream)
        {
            context.Response.Headers["Content-Encoding"] = compressor.ContentEncoding;
            context.Response.ContentLength = memoryStream.Length;
        }
    }
}