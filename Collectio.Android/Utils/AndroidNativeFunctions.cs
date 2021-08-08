using System.IO;
using Android.Graphics;
using Collectio.Droid.Utils;
using Collectio.Utils;

[assembly: Xamarin.Forms.Dependency(typeof(AndroidNativeFunctions))]
namespace Collectio.Droid.Utils
{
    public class AndroidNativeFunctions : INativeFunctions
    {
        private readonly int _quality = 50;
        
        public MemoryStream ConvertToJpeg(MemoryStream stream)
        {
            //Convert image stream into byte array
            var image = new byte[stream.Length];
            stream.Read(image, 0, image.Length);
 
            //Load the bitmap
            var resultBitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length);
 
            //Create memory stream
            var outStream = new MemoryStream();
 
            //Save the image as Jpeg
            if (resultBitmap != null)
            {
                resultBitmap.Compress(Bitmap.CompressFormat.Jpeg, _quality, outStream);

                //Return the Jpeg image as stream
                return outStream;
            }

            return stream;
        }

        public MemoryStream CompressJpeg(MemoryStream stream)
        {
            //Convert image stream into byte array
            var image = new byte[stream.Length];
            stream.Read(image, 0, image.Length);
 
            //Load the bitmap
            var resultBitmap = BitmapFactory.DecodeByteArray(image, 0, image.Length);

            //Create memory stream
            var outStream = new MemoryStream();
 
            //Save the image as Jpeg
            if (resultBitmap != null)
            {
                resultBitmap.Compress(Bitmap.CompressFormat.Jpeg, _quality, outStream);

                //Return the Jpeg image as stream
                return outStream;
            }

            return stream;
        }
    }
}