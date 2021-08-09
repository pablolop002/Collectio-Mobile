using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Collectio.Models;
using Collectio.Resources.Culture;
using Collectio.Utils;
using Microsoft.AppCenter.Analytics;
using MvvmHelpers;
using MvvmHelpers.Commands;
using Xamarin.Essentials;

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
            var selection = await Xamarin.Forms.Shell.Current.DisplayActionSheet(Strings.ImageOrigin, Strings.Cancel,
                null, Strings.Camera, Strings.Gallery);

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
                        await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
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
                        await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
                    }
                });
            }
        }

        private async Task SaveItem()
        {
            IsBusy = true;

            var original = App.DataRepo.GetCollection(_collection.Id.ToString());

            if (!string.IsNullOrWhiteSpace(_imageName))
            {
                _collection.Image = _imageName;
                FileSystemUtils.SaveFileFromPath(_file, _imageName, _collection.Id);
                FileSystemUtils.ClearTempPath();
            }

            if (!_collection.Equals(original))
            {
                _collection.UpdatedAt = DateTime.Now;
                if (await App.DataRepo.UpdateCollection(_collection))
                {
                    if (!string.IsNullOrWhiteSpace(original.Image) && !string.IsNullOrWhiteSpace(_imageName))
                    {
                        FileSystemUtils.DeleteImage(original.File);
                    }

                    await Xamarin.Forms.Shell.Current.GoToAsync("..?refresh=true");
                }
                else
                {
                    await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, "Strings.UpdateCollectionError",
                        Strings.Ok);
                }

                Analytics.TrackEvent("UpdateCollection");
            }

            IsBusy = false;
        }
    }
}