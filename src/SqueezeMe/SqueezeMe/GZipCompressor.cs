using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace SqueezeMe
{
    internal class GZipCompressor : ICompressor
    {
        public string ContentEncoding
        {
            get { return "gzip"; }
        }

        public async Task CompressAsync(HttpContent source, Stream destination)
        {
            using (var gzipStream = new GZipStream(destination, CompressionLevel.Fastest, leaveOpen: false))
            {
                await source.CopyToAsync(gzipStream).ConfigureAwait(false);
            }
        }

        public Stream CreateStream(Stream destination)
        {
            return new GZipStream(destination, CompressionLevel.Fastest, leaveOpen: false);
        }
    }
}