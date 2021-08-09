using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Collectio.Models;
using Collectio.Resources.Culture;
using Collectio.Utils;
using Microsoft.AppCenter.Analytics;
using Plugin.StoreReview;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [QueryProperty("Collection", "collection")]
    [QueryProperty("CopyFrom", "copyFrom")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
    public partial class ItemNewView : ContentPage
    {
        private readonly double _size;
        private readonly int _maxSize = 3;
        private Collection _collection;

        private readonly List<KeyValuePair<string, KeyValuePair<string, ImageButton>>> _images =
            new List<KeyValuePair<string, KeyValuePair<string, ImageButton>>>(6);

        private static int NewItemUsage
        {
            get => Preferences.Get(nameof(NewItemUsage), 0);
            set => Preferences.Set(nameof(NewItemUsage), value);
        }

        public string Collection
        {
            set
            {
                _collection = App.DataRepo.GetCollection(Uri.UnescapeDataString(value));
                SubcategoryPicker.ItemsSource =
                    new List<Subcategory>(App.DataRepo.GetSubcategoriesByCategoryId(_collection.CategoryId.ToString()));
                if (_collection.Private)
                {
                    Private.IsChecked = true;
                    Private.IsEnabled = false;
                }
            }
        }

        public string CopyFrom
        {
            set
            {
                var item = App.DataRepo.GetItem(Uri.UnescapeDataString(value), true);
                Name.Text = item.Name;
                Description.Text = item.Description;
                Private.IsChecked = item.Private;
                foreach (var image in item.Images)
                {
                    _images.Add(new KeyValuePair<string, KeyValuePair<string, ImageButton>>(image.File,
                        new KeyValuePair<string, ImageButton>(image.Image, new ImageButton
                        {
                            Source = image.File,
                            Aspect = Aspect.AspectFill,
                            WidthRequest = _size,
                            HeightRequest = _size
                        })));
                    _images[_images.Count - 1].Value.Value.Clicked += Delete_OnClicked;
                    ImagesGroup.Children.Add(_images[_images.Count - 1].Value.Value, (_images.Count - 1) % _maxSize,
                        (_images.Count - 1) / _maxSize);
                }
            }
        }

        public ItemNewView()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
            if (DeviceInfo.Idiom == DeviceIdiom.Tablet) _maxSize = 2;
            if (DeviceDisplay.MainDisplayInfo.Orientation == DisplayOrientation.Landscape)
            {
                _size = DeviceDisplay.MainDisplayInfo.Height / 2 / _maxSize;
            }
            else
            {
                _size = DeviceDisplay.MainDisplayInfo.Width / 2 / _maxSize;
            }
        }

        private async void AddImage_OnClicked(object sender, EventArgs e)
        {
            if (_images.Count > 5)
            {
                await Shell.Current.DisplayAlert(Strings.Error, "Strings.ItemImagesLimit", Strings.Ok);
                return;
            }

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
                                var image = FileSystemUtils.TempSave(memStream, photo.FileName);
                                _images.Add(new KeyValuePair<string, KeyValuePair<string, ImageButton>>(image,
                                    new KeyValuePair<string, ImageButton>(photo.FileName, new ImageButton
                                    {
                                        Source = image,
                                        Aspect = Aspect.AspectFill,
                                        WidthRequest = _size,
                                        HeightRequest = _size
                                    })));
                            }
                        }

                        var pos = _images.Count - 1;
                        _images[pos].Value.Value.Clicked += Delete_OnClicked;
                        ImagesGroup.Children.Add(_images[pos].Value.Value, pos % _maxSize, pos / _maxSize);
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
                                var image = FileSystemUtils.TempSave(memStream, photo.FileName);
                                _images.Add(new KeyValuePair<string, KeyValuePair<string, ImageButton>>(image,
                                    new KeyValuePair<string, ImageButton>(photo.FileName, new ImageButton
                                    {
                                        Source = image,
                                        Aspect = Aspect.AspectFill,
                                        WidthRequest = _size,
                                        HeightRequest = _size
                                    })));
                            }
                        }

                        var pos = _images.Count - 1;
                        _images[pos].Value.Value.Clicked += Delete_OnClicked;
                        ImagesGroup.Children.Add(_images[pos].Value.Value, pos % _maxSize, pos / _maxSize);
                    }
                    catch (PermissionException ex)
                    {
                        await Shell.Current.DisplayAlert(Strings.Error, ex.Message, Strings.Ok);
                    }
                });
            }
        }

        private void Delete_OnClicked(object sender, EventArgs e)
        {
            var aux = sender as ImageButton;
            var elem = _images.FindIndex(pair => pair.Value.Value == aux);

            _images.Remove(_images[elem]);
            MainThread.BeginInvokeOnMainThread(ImagesGroup.Children.Clear);

            var pos = 0;
            foreach (var image in _images)
            {
                var pos1 = pos;
                MainThread.BeginInvokeOnMainThread(() =>
                    ImagesGroup.Children.Add(image.Value.Value, pos1 % _maxSize, pos1 / _maxSize));
                pos++;
            }
        }

        private async void Done_OnClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Name.Text))
            {
                await Shell.Current.DisplayAlert(Strings.Error, Strings.EmptyItemName, Strings.Ok);
                return;
            }

            if (SubcategoryPicker.SelectedIndex == -1)
            {
                await Shell.Current.DisplayAlert(Strings.Error, "Strings.SubcategoryNotSelected", Strings.Ok);
                return;
            }

            var item = new Item
            {
                Name = Name.Text,
                Description = Description.Text,
                CollectionId = _collection.Id,
                Private = Private.IsChecked,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                SubcategoryId = ((Subcategory)SubcategoryPicker.SelectedItem).Id
            };

            /*foreach (var itemImage in _images.Select(image => new ItemImage
            {
                Image = image.Value.Key
            }))
            {
                item.Images.Add(itemImage);
            }*/

            var itemId = await App.DataRepo.AddItem(item);

            if (itemId == -1)
            {
                await Shell.Current.DisplayAlert(Strings.Error, "Strings.CollectionSavedError", Strings.Ok);
                return;
            }

            item.Images = new List<ItemImage>();

            foreach (var itemImage in _images.Select(image => new ItemImage
            {
                Image = image.Value.Key,
                ItemId = itemId
            }))
            {
                if (await App.DataRepo.AddItemImage(itemImage))
                {
                    FileSystemUtils.SaveFileFromPath(itemImage.TempFile, itemImage.Image, item.CollectionId, itemId);
                }
            }

            /*foreach (var image in item.Images)
            {
                FileSystemUtils.SaveFileFromPath(image.TempFile, image.Image, item.CollectionId, itemId);
            }*/

            FileSystemUtils.ClearTempPath();

            if (NewItemUsage++ == 25) await CrossStoreReview.Current.RequestReview(false);

            await Shell.Current.GoToAsync($"..");
            Analytics.TrackEvent("CreateItem");
        }
    }
}