using System;
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
    public partial class EditCollectionPage : ContentPage
    {
        private string _collectionId;
        private string _imageName = string.Empty;
        private MemoryStream _imageStream = new MemoryStream();

        public string Collection
        {
            set
            {
                _collectionId = Uri.UnescapeDataString(value);
                BindingContext = App.DataRepo.GetCollection(_collectionId);
            }
        }

        public EditCollectionPage()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
        }

        private async void Done_OnClicked(object sender, EventArgs e)
        {
            var old = App.DataRepo.GetCollection(_collectionId);
            var changed = new Collection()
            {
                Id = old.Id,
                Name = Name.Text,
                GroupId = old.GroupId,
                Description = Description.Text,
                Private = Private.IsChecked,
                Image = string.IsNullOrWhiteSpace(_imageName) ? old.Image : _imageName
            };

            if (!old.Equals(changed))
            {
                App.DataRepo.UpdateCollection(changed);
            }

            if (!string.IsNullOrWhiteSpace(_imageName) && !string.IsNullOrWhiteSpace(old.Image))
            {
                FileSystemUtils.DeleteImage(old.File);
                FileSystemUtils.SaveFileFromStream(_imageStream, _imageName, changed.Id);
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
                        stream.Close();
                        stream.Dispose();
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