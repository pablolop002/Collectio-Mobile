using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Collectio.Models;
using Collectio.Resources.Culture;
using Collectio.Utils;
using MvvmHelpers.Commands;
using Xamarin.Essentials;

namespace Collectio.ViewModels.BaseViewModels
{
    public class BaseCollectionViewModel : ViewModelBase
    {
        private Collection _collection;
        private string _file;
        private string _imageName = string.Empty;

        public ObservableCollection<Category> Categories { get; set; }

        public ICommand ChangeImageCommand { get; }

        public Collection Collection
        {
            get => _collection;
            set => SetProperty(ref _collection, value);
        }

        public string File
        {
            get => _file;
            set => SetProperty(ref _file, value);
        }

        public string ImageName
        {
            get => _imageName;
            set => SetProperty(ref _imageName, value);
        }

        public BaseCollectionViewModel()
        {
            Categories = new ObservableCollection<Category>(App.DataRepo.GetCategories());
            ChangeImageCommand = new AsyncCommand(ChangeImage);
            Collection = new Collection();
        }

        private async Task ChangeImage()
        {
            FileResult image;
            var selection = await MainThread.InvokeOnMainThreadAsync(() =>
                Xamarin.Forms.Shell.Current.DisplayActionSheet(
                    Strings.ImageOrigin, Strings.Cancel, null,
                    Strings.Camera, Strings.Gallery, Strings.Picker));

            if (selection == null || selection == Strings.Cancel) return;
            if (selection == Strings.Camera)
            {
                image = await PhotoCapture();
            }
            else if (selection == Strings.Gallery)
            {
                image = await GallerySelector();
            }
            else
            {
                image = await ImagePicker();
            }

            if (image == null) return;
            
            using (var stream = await image.OpenReadAsync())
            {
                using (var memStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memStream);
                    File = FileSystemUtils.TempSave(memStream, image.FileName);
                    
                    _imageName = image.FileName.EndsWith(".png") ? image.FileName.Replace(".png", ".jpg") : image.FileName;
                }
            }
        }
    }
}