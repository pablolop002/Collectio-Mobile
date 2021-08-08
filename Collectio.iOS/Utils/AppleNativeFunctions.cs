using System.IO;
using Collectio.iOS.Utils;
using Collectio.Utils;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(AppleNativeFunctions))]
namespace Collectio.iOS.Utils
{
    public class AppleNativeFunctions : INativeFunctions
    {
        private readonly int _quality = 50;

        public MemoryStream ConvertToJpeg(MemoryStream stream)
        {
            //Convert image stream into byte array
            var image = new byte[stream.Length];
            stream.Read(image, 0, image.Length);
 
            //Load the image
            var images = new UIImage(Foundation.NSData.FromArray(image));
 
            //Save the image as Jpeg
            var bytes = images.AsJPEG(_quality).ToArray();
 
            //Store the byte array into memory stream
            var imgStream = new MemoryStream(bytes);
 
            //Return the Jpeg image as stream
            return imgStream;
        }

        public MemoryStream CompressJpeg(MemoryStream stream)
        {
            //Load the image
            var nsData = Foundation.NSData.FromStream(stream);
            var images = UIImage.LoadFromData(nsData);
 
            //Save the image as Jpeg
            var bytes = images.AsJPEG(_quality).ToArray();
 
            //Store the byte array into memory stream
            var imgStream = new MemoryStream(bytes);
 
            //Return the Jpeg image as stream
            return imgStream;
        }
    }
}