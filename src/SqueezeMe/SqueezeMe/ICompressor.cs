using System.IO;

namespace SqueezeMe
{
    public interface ICompressor
    {
        string ContentEncoding { get; }

        Stream CreateStream(Stream destination);
        Stream Decompress(Stream source);
    }
}