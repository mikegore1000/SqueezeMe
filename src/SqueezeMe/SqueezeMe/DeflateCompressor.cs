using System.IO;
using System.IO.Compression;

namespace SqueezeMe
{
    public class DeflateCompressor : ICompressor
    {
        public string ContentEncoding => "deflate";

        public Stream CreateStream(Stream destination)
        {
            return new DeflateStream(destination, CompressionLevel.Fastest, leaveOpen: true);
        }
    }
}