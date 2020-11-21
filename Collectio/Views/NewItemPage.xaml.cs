using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Collectio.Models;
using Collectio.Resources.Culture;
using Collectio.Utils;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [QueryProperty("Collection", "collection")]
    [QueryProperty("CopyFrom", "copyFrom")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewItemPage : ContentPage
    {
        private Collection _collection;
        private Item _item;
        private List<KeyValuePair<string, MemoryStream>> _images;

        public string Collection
        {
            set
            {
                _collection = App.DataRepo.GetCollection(Uri.UnescapeDataString(value));
                SubcategoryPicker.ItemsSource =
                    new List<Subcategory>(App.DataRepo.GetSubcategoriesByCategoryId(_collection.CategoryId.ToString()));
            }
        }

        public string CopyFrom
        {
            set
            {
                _item = App.DataRepo.GetItem(Uri.UnescapeDataString(value), true);
                Name.Text = _item.Name;
                Description.Text = _item.Description;
                Private.IsChecked = _item.Private;
            }
        }

        public NewItemPage()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
            _images = new List<KeyValuePair<string, MemoryStream>>();
            Images.ItemsSource = _images;
        }

        private async void AddImage_OnClicked(object sender, EventArgs e)
        {
            if (_images.Count == 5)
            {
                await Shell.Current.DisplayAlert(Strings.Error, "Limit Reached", Strings.Ok);
                return;
            }

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

                        var stream = await photo.OpenReadAsync();
                        var imageStream = new MemoryStream();
                        await stream.CopyToAsync(imageStream);
                        _images.Add(new KeyValuePair<string, MemoryStream>(photo.FileName, imageStream));
                        Images.ItemsSource = _images;
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

                        var stream = await photo.OpenReadAsync();
                        var imageStream = new MemoryStream();
                        await stream.CopyToAsync(imageStream);
                        _images.Add(new KeyValuePair<string, MemoryStream>(photo.FileName, imageStream));
                        Images.ItemsSource = _images;
                    }
                    catch (PermissionException ex)
                    {
                        await Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
                    }
                });
            }
        }

        private async void Done_OnClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Name.Text))
            {
                await Shell.Current.DisplayAlert(Strings.Error, Strings.EmptyItemName, Strings.Ok);
                return;
            }

            var item = new Item
            {
                Name = Name.Text,
                Description = Description.Text,
                CollectionId = _collection.Id,
                Private = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                SubcategoryId = ((Subcategory) SubcategoryPicker.SelectedItem).Id
            };

            App.DataRepo.AddItem(ref item);

            foreach (var itemImage in from image in _images
                let correct = FileSystemUtils.SaveFileFromStream(image.Value, image.Key, item.CollectionId, item.Id)
                where correct
                select new ItemImage()
                {
                    ItemId = item.Id,
                    Image = image.Key
                })
            {
                App.DataRepo.AddItemImage(itemImage);
            }

            await Shell.Current.GoToAsync($"..?collection={_collection.Id}&refresh=true");
        }
    }
}