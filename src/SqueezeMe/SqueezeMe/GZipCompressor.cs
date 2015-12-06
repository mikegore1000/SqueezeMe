using System.IO;
using System.IO.Compression;

namespace SqueezeMe
{
    public class GZipCompressor : ICompressor
    {
        public string ContentEncoding
        {
            get { return "gzip"; }
        }

        public Stream CreateStream(Stream destination)
        {
            return new GZipStream(destination, CompressionLevel.Fastest, leaveOpen: true);
        }
    }
}