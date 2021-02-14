using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Collectio.Models;
using Collectio.Resources.Culture;
using Collectio.Utils;
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
                        new KeyValuePair<string, ImageButton>(image.Image, new ImageButton()
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
                await Shell.Current.DisplayAlert(Strings.Error, "Limit Reached", Strings.Ok);
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
                                    new KeyValuePair<string, ImageButton>(photo.FileName, new ImageButton()
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
                                    new KeyValuePair<string, ImageButton>(photo.FileName, new ImageButton()
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

            var item = new Item
            {
                Name = Name.Text,
                Description = Description.Text,
                CollectionId = _collection.Id,
                Private = Private.IsChecked,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                SubcategoryId = SubcategoryPicker.SelectedIndex != -1 ? ((Subcategory) SubcategoryPicker.SelectedItem).Id : -1
            };

            App.DataRepo.AddItem(ref item);

            foreach (var itemImage in from image in _images
                let correct = FileSystemUtils.SaveFileFromPath(image.Key, image.Value.Key, item.CollectionId, item.Id)
                where correct
                select new ItemImage()
                {
                    ItemId = item.Id,
                    Image = image.Value.Key
                })
            {
                App.DataRepo.AddItemImage(itemImage);
            }

            FileSystemUtils.ClearTempPath();
            
            if (NewItemUsage++ == 25) await CrossStoreReview.Current.RequestReview(false);

            await Shell.Current.GoToAsync($"..?collection={_collection.Id}&refresh=true");
        }
    }
}