using System.IO;

namespace Collectio.Utils
{
    public interface INativeFunctions
    {
        Stream ConvertToJpeg(Stream stream);
    }
}