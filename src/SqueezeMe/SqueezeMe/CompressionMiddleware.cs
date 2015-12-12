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
        private const string AcceptEncoding = "Accept-Encoding";
        private const int BufferSize = 8192;
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
            var compressor = GetCompressor(context.Request);

            if (compressor == null)
            {
                await next.Invoke(environment);
                return;
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var compressedStream = compressor.CreateStream(memoryStream))
                    {
                        context.Response.Body = compressedStream;
                        await next.Invoke(environment);
                    }

                    if (memoryStream.Length > 0)
                    {
                        SetResponseHeaders(context, compressor, memoryStream);
                        memoryStream.Position = 0;
                        await memoryStream.CopyToAsync(httpOutputStream, BufferSize);
                    }
                }
            }
            finally
            {
                context.Response.Body = httpOutputStream;
            }
        }

        private static void SetResponseHeaders(OwinContext context, ICompressor compressor, MemoryStream memoryStream)
        {
            context.Response.Headers["Content-Encoding"] = compressor.ContentEncoding;
            context.Response.ContentLength = memoryStream.Length;
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
