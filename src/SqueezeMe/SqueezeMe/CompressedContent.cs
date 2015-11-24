using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SqueezeMe
{
    public class CompressedContent : HttpContent
    {
        private readonly HttpContent content;
        private readonly ICompressor compressor;

        public CompressedContent(HttpContent content, ICompressor compressor)
        {
            this.content = content;
            this.compressor = compressor;
            CopyHeaders();
            AddCompressionHeaders();
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            await compressor.CompressAsync(this.content, stream).ConfigureAwait(false);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        private void CopyHeaders()
        {
            foreach (var header in content.Headers)
            {
                Headers.Add(header.Key, header.Value);
            }
        }

        private void AddCompressionHeaders()
        {
            Headers.ContentEncoding.Add(compressor.ContentEncoding);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                content.Dispose();
            }
        }
    }
}