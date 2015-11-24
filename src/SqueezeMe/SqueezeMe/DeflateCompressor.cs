using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace SqueezeMe
{
    public class DeflateCompressor : ICompressor
    {
        public string ContentEncoding
        {
            get { return "deflate"; }
        }

        public async Task CompressAsync(HttpContent source, Stream destination)
        {
            using (var deflateStream = new DeflateStream(destination, CompressionLevel.Fastest, leaveOpen: false))
            {
                await source.CopyToAsync(deflateStream).ConfigureAwait(false);
            }
        }

        public Stream CreateStream(Stream destination)
        {
            return new DeflateStream(destination, CompressionLevel.Fastest, leaveOpen: false);
        }
    }
}