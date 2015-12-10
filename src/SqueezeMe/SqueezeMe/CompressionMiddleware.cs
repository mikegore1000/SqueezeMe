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
        private const string AcceptEncoding = "Accept-Encoding";
        private readonly Func<IDictionary<string, object>, Task> next;

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

            try
            {
                using (var responseStream = new MemoryStream())
                {
                    context.Response.Body = responseStream;

                    await next.Invoke(environment);

                    var compressor = GetCompressor(context.Request);

                    responseStream.Position = 0;
                    if (context.Response.ContentLength.HasValue)
                    {
                        if (compressor != null)
                        {

                            await Compress(context, httpOutputStream, responseStream, compressor);
                        }
                        else
                        {
                            context.Response.ContentLength = responseStream.Length;
                            await responseStream.CopyToAsync(httpOutputStream);
                        }
                    }
                }
            }
            finally
            {
                context.Response.Body = httpOutputStream;
            }
        }

        private static async Task Compress(OwinContext context, Stream httpOutputStream, MemoryStream responseStream, ICompressor compressor)
        {
            using (var compressedStream = new MemoryStream())
            {
                using (var compressionStream = compressor.CreateStream(compressedStream))
                {
                    await responseStream.CopyToAsync(compressionStream, BufferSize);
                }

                context.Response.Headers["Content-Encoding"] = compressor.ContentEncoding;
                context.Response.ContentLength = compressedStream.Length;

                compressedStream.Position = 0;
                compressedStream.CopyTo(httpOutputStream);
            }
        }

        private ICompressor GetCompressor(IOwinRequest request)
        {
            if(!request.Headers.ContainsKey(AcceptEncoding))
            {
                return null;
            }

            return (from c in compressors
                    from e in request.Headers.GetCommaSeparatedValues(AcceptEncoding).Select(x => StringWithQualityHeaderValue.Parse(x))
                    orderby e.Quality descending
                    where string.Compare(c.ContentEncoding, e.Value, StringComparison.InvariantCultureIgnoreCase) == 0
                    select c).FirstOrDefault();
        }
    }
}
