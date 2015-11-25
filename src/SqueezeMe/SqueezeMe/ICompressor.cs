using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SqueezeMe
{
    public interface ICompressor
    {
        string ContentEncoding { get; }

        Stream CreateStream(Stream destination);
    }
}