using System.IO;

namespace Collectio.Utils
{
    public interface INativeFunctions
    {
        MemoryStream ConvertToJpeg(MemoryStream stream);
        
        MemoryStream CompressJpeg(MemoryStream stream);
    }
}