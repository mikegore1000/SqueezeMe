using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SqueezeMe
{
    internal class CompressedContent : HttpContent
    {
        private readonly HttpContent content;
        private readonly ICompressor compressor;

        public CompressedContent(HttpContent content, ICompressor compressor)
        {
            this.content = content;
            this.compressor = compressor;
            AddHeaders();
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            using (var compressionStream = compressor.CreateStream(stream))
            {
                await content.CopyToAsync(compressionStream).ConfigureAwait(false);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        private void AddHeaders()
        {
            foreach (var header in content.Headers)
            {
                Headers.Add(header.Key, header.Value);
            }

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