namespace SqueezeMe
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using System.IO;
    using System.Net.Http.Headers;

    public class CompressionMiddleware
    {
        private const int BufferSize = 1024 * 4;
        private Func<IDictionary<string, object>, Task> next;

        private readonly List<ICompressor> compressors = new List<ICompressor>()
        {
            new GZipCompressor(),
            new DeflateCompressor()
        };

        public CompressionMiddleware(Func<IDictionary<string, object>, Task> next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var context = new OwinContext(environment);
            var httpOutputStream = context.Response.Body;

            using (var responseStream = new MemoryStream())
            {
                context.Response.Body = responseStream;

                await next.Invoke(environment);
                var compressor = GetCompressor(context.Request);

                if (compressor != null && context.Response.Body != null)
                {
                    await Compress(context, httpOutputStream, responseStream, compressor);
                }
            }

            context.Response.Body = httpOutputStream;
        }

        private static async Task Compress(OwinContext context, Stream httpOutputStream, MemoryStream responseStream, ICompressor compressor)
        {
            responseStream.Position = 0;
            long contentLength = 0;

            using (var compressedStream = new MemoryStream())
            {
                using (var compressionStream = compressor.CreateStream(compressedStream))
                {
                    await responseStream.CopyToAsync(compressionStream, BufferSize);
                }

                contentLength = compressedStream.Length;
                compressedStream.Position = 0;
                compressedStream.CopyTo(httpOutputStream);
            }

            context.Response.Headers["Content-Encoding"] = compressor.ContentEncoding;
            context.Response.ContentLength = contentLength;
        }

        private ICompressor GetCompressor(IOwinRequest request)
        {
            return (from c in compressors
                    from e in request.Headers.GetCommaSeparatedValues("Accept-Encoding").Select(x => new StringWithQualityHeaderValue(x))
                    orderby e.Quality descending
                    where string.Compare(c.ContentEncoding, e.Value, StringComparison.InvariantCultureIgnoreCase) == 0
                    select c).FirstOrDefault();
        }
    }
}
