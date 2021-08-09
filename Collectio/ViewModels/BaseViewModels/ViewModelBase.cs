using System.Threading.Tasks;
using Collectio.Resources.Culture;
using Collectio.Utils;
using MvvmHelpers;
using Xamarin.Essentials;

namespace Collectio.ViewModels.BaseViewModels
{
    public class ViewModelBase : BaseViewModel
    {
        protected async Task<FileResult> GallerySelector()
        {
            try
            {
                return await MainThread.InvokeOnMainThreadAsync(() => MediaPicker.PickPhotoAsync());
            }
            catch (PermissionException ex)
            {
#if DEBUG
                await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
#endif
                AppCenterUtils.ReportException(ex, "GallerySelector");
                return null;
            }
        }

        protected async Task<FileResult> PhotoCapture()
        {
            try
            {
                return await MainThread.InvokeOnMainThreadAsync(() => MediaPicker.CapturePhotoAsync());
            }
            catch (PermissionException ex)
            {
#if DEBUG
                await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
#endif
                AppCenterUtils.ReportException(ex, "PhotoCapture");
                return null;
            }
        }

        protected async Task<FileResult> ImagePicker()
        {
            try
            {
                return await MainThread.InvokeOnMainThreadAsync(() => FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Jpeg
                }));
            }
            catch (PermissionException ex)
            {
#if DEBUG
                await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
#endif
                AppCenterUtils.ReportException(ex, "ImagePicker");
                return null;
            }
        }
    }
}