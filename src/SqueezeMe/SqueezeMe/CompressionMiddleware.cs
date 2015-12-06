using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace SqueezeMe
{
    using System.Diagnostics;
    using System.IO;
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class CompressionMiddleware
    {
        private AppFunc next;

        private readonly List<ICompressor> compressors = new List<ICompressor>()
        {
            new GZipCompressor(),
            new DeflateCompressor()
        };

        public CompressionMiddleware(AppFunc next)
        {
            this.next = next;
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            var context = new OwinContext(environment);
            var destinationStream = context.Response.Body;
            long contentLength = 0;

            using (var responseStream = new MemoryStream())
            {
                context.Response.Body = responseStream;

                await next.Invoke(environment);
                var compressor = GetCompressor(context.Request);

                if (compressor != null && context.Response.Body != null)
                {
                    responseStream.Position = 0;
                    contentLength = responseStream.Length;

                    // COMPRESS!!!
                    using (var compressedStream = new MemoryStream())
                    {
                        using (var compressionStream = compressor.CreateStream(compressedStream))
                        {
                            await responseStream.CopyToAsync(compressionStream, 1024 * 4);
                        }

                        contentLength = compressedStream.Length;
                        compressedStream.Position = 0;
                        compressedStream.CopyTo(destinationStream);
                    }
                }

                context.Response.Headers["Content-Encoding"] = compressor.ContentEncoding;
            }

            context.Response.Body = destinationStream;
            context.Response.ContentLength = contentLength;
        }

        private ICompressor GetCompressor(IOwinRequest request)
        {
            return (from c in compressors
                    from e in request.Headers.GetCommaSeparatedValues("Accept-Encoding").Select(x => {
                        // TODO: Need to tidy this up
                        var valueAndQuality = x.Split(';');
                        var value = valueAndQuality.Length > 0 ? valueAndQuality[0] : null;
                        var quality = valueAndQuality.Length > 1 ? float.Parse(valueAndQuality[1].Substring(2)) : 1;

                        return new { Value = value, Quality = quality };
                    })
                    orderby e.Quality descending
                    where string.Compare(c.ContentEncoding, e.Value, StringComparison.InvariantCultureIgnoreCase) == 0
                    select c).FirstOrDefault();
        }
    }
}
