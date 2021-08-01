using System.IO;
using Android.Graphics;
using Collectio.Utils;

[assembly: Xamarin.Forms.Dependency(typeof(INativeFunctions))]
namespace Collectio.Droid.Utils
{
    public class AndroidNativeFunctions : INativeFunctions
    {
        private readonly int _quality = 50;
        
        public MemoryStream ConvertToJpeg(Stream stream)
        {
            //Convert image stream into byte array
            var image = new byte[stream.Length];
            stream.Read(image, 0, image.Length);
 
            //Load the bitmap
            var resultBitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length);
 
            //Create memory stream
            var outStream = new MemoryStream();
 
            //Save the image as Jpeg
            resultBitmap?.Compress(Bitmap.CompressFormat.Jpeg, _quality, outStream);

            //Return the Jpeg image as stream
            return outStream;
        }

        public MemoryStream CompressJpeg(Stream stream)
        {
            //Load the bitmap
            var resultBitmap = BitmapFactory.DecodeStream(stream);
            
            //Create memory stream
            var outStream = new MemoryStream();
 
            //Save the image as Jpeg
            resultBitmap?.Compress(Bitmap.CompressFormat.Jpeg, _quality, outStream);

            //Return the Jpeg image as stream
            return outStream;
        }
    }
}