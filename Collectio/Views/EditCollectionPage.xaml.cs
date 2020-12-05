using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Collectio.Models;
using Collectio.Resources.Culture;
using Collectio.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [QueryProperty("Collection", "collection")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
    public partial class EditCollectionPage : ContentPage
    {
        private Collection _collection;
        private string _imageName = string.Empty;
        private string _file = string.Empty;

        public string Collection
        {
            set => BindingContext = _collection = App.DataRepo.GetCollection(value, true);
        }

        public EditCollectionPage()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
        }

        private async void Done_OnClicked(object sender, EventArgs e)
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

            await Shell.Current.GoToAsync("..?refresh=true");
        }

        private async void SelectImage_OnClicked(object sender, EventArgs e)
        {
            var selection = await Shell.Current.DisplayActionSheet(Strings.ImageOrigin, Strings.Cancel, null,
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

                        Image.Source = ImageSource.FromFile(_file);
                    }
                    catch (PermissionException ex)
                    {
                        await Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
                    }
                });
            }
            else
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

                        Image.Source = ImageSource.FromFile(_file);
                    }
                    catch (PermissionException ex)
                    {
                        await Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
                    }
                });
            }
        }
    }
}