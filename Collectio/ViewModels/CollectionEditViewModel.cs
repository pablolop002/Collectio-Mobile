using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Collectio.Models;
using Collectio.Resources.Culture;
using Collectio.Utils;
using MvvmHelpers;
using MvvmHelpers.Commands;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Collectio.ViewModels
{
    public class CollectionEditViewModel : BaseViewModel
    {
        private Collection _collection;
        private string _imageName = string.Empty;
        private string _file = string.Empty;

        public Collection Collection
        {
            get => _collection;
            set => SetProperty(ref _collection, value);
        }

        public ICommand ImageSelectorCommand { get; }
        
        public ICommand SaveCommand { get; }

        public CollectionEditViewModel()
        {
            ImageSelectorCommand = new AsyncCommand(ImageSelector);
            SaveCommand = new AsyncCommand(SaveItem);
        }

        private async Task ImageSelector()
        {
            var selection = await Xamarin.Forms.Shell.Current.DisplayActionSheet(Strings.ImageOrigin, Strings.Cancel, null,
                Strings.Camera, Strings.Gallery);

            if (selection == null || selection == Strings.Cancel) return;
            if (selection == Strings.Camera)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        var photo = await MediaPicker.CapturePhotoAsync();
                        if (photo == null) return;

                        using (var stream = await photo.OpenReadAsync())
                        {
                            using (var memStream = new MemoryStream())
                            {
                                await stream.CopyToAsync(memStream);
                                _file = FileSystemUtils.TempSave(memStream, photo.FileName);
                                _imageName = photo.FileName;
                            }
                        }

                        //Image.Source = ImageSource.FromFile(_file);
                    }
                    catch (PermissionException ex)
                    {
                        await Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
                    }
                });
            }
            else if (selection == Strings.Gallery)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        var photo = await MediaPicker.PickPhotoAsync();
                        if (photo == null) return;

                        using (var stream = await photo.OpenReadAsync())
                        {
                            using (var memStream = new MemoryStream())
                            {
                                await stream.CopyToAsync(memStream);
                                _file = FileSystemUtils.TempSave(memStream, photo.FileName);
                                _imageName = photo.FileName;
                            }
                        }

                        //Image.Source = ImageSource.FromFile(_file);
                    }
                    catch (PermissionException ex)
                    {
                        await Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
                    }
                });
            }
        }

        private async Task SaveItem()
        {
            var original = App.DataRepo.GetCollection(_collection.Id.ToString());

            if (!string.IsNullOrWhiteSpace(_imageName))
            {
                if (!string.IsNullOrWhiteSpace(original.Image))
                {
                    FileSystemUtils.DeleteImage(original.File);
                }

                _collection.Image = _imageName;
                FileSystemUtils.SaveFileFromPath(_file, _imageName, _collection.Id);
                FileSystemUtils.ClearTempPath();
            }

            if (!_collection.Equals(original))
            {
                _collection.UpdatedAt = DateTime.Now;
                App.DataRepo.UpdateCollection(_collection);
            }

            await Xamarin.Forms.Shell.Current.GoToAsync("..?refresh=true");
        }
    }
}