using System.IO;
using Collectio.Utils;
using UIKit;

[assembly: Xamarin.Forms.Dependency(typeof(INativeFunctions))]
namespace Collectio.iOS.Utils
{
    public class AppleNativeFunctions : INativeFunctions
    {
        public Stream ConvertToJpeg(Stream stream)
        {
            //Convert image stream into byte array
            var image = new byte[stream.Length];
            stream.Read(image, 0, image.Length);
 
            //Load the image
            var images = new UIImage(Foundation.NSData.FromArray(image));
 
            //Save the image as Jpeg
            var bytes = images.AsJPEG(100).ToArray();
 
            //Store the byte array into memory stream
            Stream imgStream = new MemoryStream(bytes);
 
            //Return the Jpeg image as stream
            return imgStream;
        }
    }
}