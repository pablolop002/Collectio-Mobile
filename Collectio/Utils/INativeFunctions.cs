using System.IO;

namespace Collectio.Utils
{
    public interface INativeFunctions
    {
        MemoryStream ConvertToJpeg(Stream stream);
        
        MemoryStream CompressJpeg(Stream stream);
    }
}