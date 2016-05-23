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

        public Stream Decompress(Stream source)
        {
            return new DeflateStream(source, CompressionMode.Decompress);
        }
    }
}