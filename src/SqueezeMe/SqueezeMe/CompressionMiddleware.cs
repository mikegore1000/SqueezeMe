using SqueezeMe.CompressionStrategies;

namespace SqueezeMe
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Owin;
    using System.Net.Http.Headers;

    public class CompressionMiddleware
    {
        private const string AcceptEncoding = "Accept-Encoding";
        private readonly Func<IDictionary<string, object>, Task> next;
        private readonly CompressionStrategy compressionStrategy;

        private readonly List<ICompressor> compressors = new List<ICompressor>()
        {
            new GZipCompressor(),
            new DeflateCompressor()
        };

        public CompressionMiddleware(Func<IDictionary<string, object>, Task> next, IEnumerable<string> excludedMimeTypes)
        {
            this.next = next;
            this.compressionStrategy = GetStrategy(excludedMimeTypes);
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

            await compressionStrategy.Compress(next, context, compressor, httpOutputStream);
            context.Response.Body = httpOutputStream;
        }

        private static CompressionStrategy GetStrategy(IEnumerable<string> excludedMimeTypes)
        {
            if (excludedMimeTypes == null || !excludedMimeTypes.Any())
            {
                return new DirectCompressionStrategy();
            }

            return new ConfigurableCompressionStrategy(excludedMimeTypes);
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
