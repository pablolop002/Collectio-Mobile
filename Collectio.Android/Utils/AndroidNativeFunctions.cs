using System.IO;
using Android.Graphics;
using Collectio.Utils;

[assembly: Xamarin.Forms.Dependency(typeof(INativeFunctions))]
namespace Collectio.Droid.Utils
{
    public class AndroidNativeFunctions : INativeFunctions
    {
        public Stream ConvertToJpeg(Stream stream)
        {
            //Convert image stream into byte array
            var image = new byte[stream.Length];
            stream.Read(image, 0, image.Length);
 
            //Load the bitmap
            var resultBitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length);
 
            //Create memory stream
            var outStream = new MemoryStream();
 
            //Save the image as Jpeg
            resultBitmap?.Compress(Bitmap.CompressFormat.Jpeg, 100, outStream);

            //Return the Jpeg image as stream
            return outStream;
        }
    }
}