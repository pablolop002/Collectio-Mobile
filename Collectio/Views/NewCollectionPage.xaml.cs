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
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewCollectionPage : ContentPage
    {
        private Group _group;
        private Collection _collection;
        private string _imageName = string.Empty;
        private MemoryStream _imageStream = new MemoryStream();

        public NewCollectionPage()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);

            _collection = new Collection();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                CategorySelector.ItemsSource = App.DataRepo.GetCollectionGroupTypes();
                CategorySelector.Focus();
                if (DeviceInfo.Idiom != DeviceIdiom.Tablet) return;
                CategorySelector.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)
                {
                    HorizontalItemSpacing = 10, VerticalItemSpacing = 10
                };
            });
        }

        private void CategorySelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _group = CategorySelector.SelectedItem as Group;
            if (_group == null) return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                CategorySelector.Unfocus();
                CategorySelector.IsVisible = false;
                ImageBorder.IsVisible = Image.IsVisible = SelectedCategory.IsVisible = Name.IsVisible =
                    Description.IsVisible = PrivateLabel.IsVisible = Private.IsVisible = true;

                SelectedCategory.Text = _group.Name;
                var done = new ToolbarItem()
                {
                    IconImageSource = new FontImageSource() {FontFamily = "FA-S", Glyph = "\uf00c"},
                    Command = new Command(CreateCollection)
                };
                ToolbarItems.Add(done);
            });
        }

        private async void CreateCollection()
        {
            if (string.IsNullOrWhiteSpace(Name.Text))
            {
                await Shell.Current.DisplayAlert(Strings.Error, Strings.EmptyCollectionName, Strings.Ok);
                return;
            }

            _collection.Name = Name.Text;
            if(!string.IsNullOrWhiteSpace(_imageName))
            {
                _collection.Image = _imageName;
            }
            _collection.Description = Description.Text;
            _collection.GroupId = _group.Id;
            _collection.Private = Private.IsChecked;
            
            App.DataRepo.AddCollection(ref _collection);
            
            if(!string.IsNullOrWhiteSpace(_imageName))
            {
                FileSystemUtils.SaveFileFromStream(_imageStream, _imageName, _collection.Id);
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