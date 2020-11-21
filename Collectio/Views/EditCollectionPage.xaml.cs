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
        private MemoryStream _imageStream = new MemoryStream();

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

            if (!_collection.Equals(original))
            {
                App.DataRepo.UpdateCollection(_collection);

                if (!string.IsNullOrWhiteSpace(_imageName))
                {
                    if(!string.IsNullOrWhiteSpace(original.Image))
                    {
                        FileSystemUtils.DeleteImage(original.File);
                    }
                    
                    FileSystemUtils.SaveFileFromStream(_imageStream, _imageName, _collection.Id);
                }
            }

            await Shell.Current.GoToAsync("..?refresh=true");
        }

        private async void SelectImage_OnClicked(object sender, EventArgs e)
        {
            var selection = await Shell.Current.DisplayActionSheet("Image", Strings.Cancel, null,
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

                        _imageName = photo.FileName;
                        var stream = await photo.OpenReadAsync();
                        await stream.CopyToAsync(_imageStream);
                        _collection.Image = _imageName;
                        Image.Source = ImageSource.FromStream(() => stream);
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

                        _imageName = photo.FileName;
                        var stream = await photo.OpenReadAsync();
                        await stream.CopyToAsync(_imageStream);
                        _collection.Image = _imageName;
                        Image.Source = ImageSource.FromFile(photo.FullPath);
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