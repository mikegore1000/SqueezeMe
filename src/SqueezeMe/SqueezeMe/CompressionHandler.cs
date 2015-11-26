using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SqueezeMe
{
    public class CompressionHandler : DelegatingHandler
    {
        private readonly List<ICompressor> compressors = new List<ICompressor>()
        {
            new GZipCompressor(),
            new DeflateCompressor()
        };

        private Func<HttpResponseMessage, bool> enableCompression;

        public CompressionHandler() : this(r => true)
        {
        }

        public CompressionHandler(Func<HttpResponseMessage, bool> enableCompression)
        {
            this.enableCompression = enableCompression;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            var baseContent = response.Content;
            var compressor = GetCompressor(request);

            if (baseContent != null && compressor != null && enableCompression(response))
            {
                response.Content = new CompressedContent(baseContent, compressor);
            }

            return response;
        }

        private ICompressor GetCompressor(HttpRequestMessage request)
        {
            return (from c in compressors 
                    from e in request.Headers.AcceptEncoding.OrderByDescending(x => x.Quality) 
                    where string.Compare(c.ContentEncoding, e.Value, StringComparison.InvariantCultureIgnoreCase) == 0 
                    select c).FirstOrDefault();
        }
    }
}