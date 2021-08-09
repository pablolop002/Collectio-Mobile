using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Collectio.Models;
using Collectio.Resources.Culture;
using Collectio.Utils;
using Microsoft.AppCenter.Analytics;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [QueryProperty("Item", "item")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
    public partial class ItemEditView : ContentPage
    {
        private readonly double _size;
        private readonly int _maxSize = 3;
        private Item _item;

        private readonly List<KeyValuePair<string, KeyValuePair<string, ImageButton>>> _images =
            new List<KeyValuePair<string, KeyValuePair<string, ImageButton>>>();

        private readonly List<string> _toDelete = new List<string>();

        public string Item
        {
            set
            {
                BindingContext = _item = App.DataRepo.GetItem(value, true);

                if (App.DataRepo.GetCollection(_item.CollectionId.ToString()).Private)
                {
                    PrivateSelector.IsEnabled = false;
                    PrivateSelector.IsChecked = true;
                }

                foreach (var image in _item.Images)
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

                var subcategoryList =
                    App.DataRepo.GetSubcategoriesByCategoryId(App.DataRepo
                        .GetCollection(_item.CollectionId.ToString())
                        .CategoryId.ToString()) as List<Subcategory>;
                SubcategoryPicker.ItemsSource = subcategoryList;
                if (SubcategoryPicker.ItemsSource != null && subcategoryList != null)
                {
                    SubcategoryPicker.SelectedIndex =
                        SubcategoryPicker.ItemsSource.IndexOf(
                            subcategoryList.Find(e => e.Id == _item.SubcategoryId));
                }
            }
        }

        public ItemEditView()
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

            _toDelete.Add(_images[elem].Key);
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
            if (string.IsNullOrWhiteSpace(_item.Name))
            {
                await Shell.Current.DisplayAlert(Strings.Error, Strings.EmptyItemName, Strings.Ok);
                return;
            }

            if (SubcategoryPicker.SelectedIndex == -1)
            {
                await Shell.Current.DisplayAlert(Strings.Error, "Strings.SubcategoryNotSelected", Strings.Ok);
                return;
            }

            var original = App.DataRepo.GetItem(_item.Id.ToString(), true);

            var toDelete = _toDelete.Select(image => _item.Images.Find(elem => elem.File == image)).ToList();
            var toAdd = _images.Where(image => !_item.Images.Exists(elem => elem.File == image.Key))
                .Select(image => new ItemImage { ItemId = _item.Id, Image = image.Value.Key }).ToList();

            if (!_item.Equals(original) || toDelete.Count > 0 || toAdd.Count > 0)
            {
                _item.UpdatedAt = DateTime.Now;

                if (await App.DataRepo.UpdateItem(_item /*, toAdd, toDelete*/))
                {
                    foreach (var image in toDelete)
                    {
                        if (await App.DataRepo.RemoveItemImage(image.Id.ToString()))
                        {
                            FileSystemUtils.DeleteImage(image.File);
                        }
                    }

                    foreach (var image in toAdd)
                    {
                        if (await App.DataRepo.AddItemImage(image))
                        {
                            FileSystemUtils.SaveFileFromPath(image.TempFile, image.Image, _item.CollectionId,
                                image.ItemId);
                        }
                    }

                    FileSystemUtils.ClearTempPath();

                    Analytics.TrackEvent("EditItem");
                    await Shell.Current.GoToAsync($"..?collection={_item.CollectionId.ToString()}&refresh=true");
                }
                else
                {
                    await Shell.Current.DisplayAlert(Strings.Error, "Strings.EditItemError", Strings.Ok);
                }
            }

            /*foreach (var image in _toDelete)
            {
                if (_item.Images.Exists(elem => elem.File == image))
                {
                    var img = _item.Images.Find(elem => elem.File == image);
                    await App.DataRepo.RemoveItemImage(img.Id.ToString());
                    _item.Images.Remove(img);
                }

                FileSystemUtils.DeleteImage(image);
            }

            foreach (var image in _images.Where(image =>
                !_item.Images.Exists(elem => elem.File == image.Key)))
            {
                FileSystemUtils.SaveFileFromPath(image.Key, image.Value.Key, _item.CollectionId, _item.Id);
                var itemImage = new ItemImage
                {
                    ItemId = _item.Id,
                    Image = image.Value.Key
                };

                App.DataRepo.AddItemImage(itemImage);
                _item.Images.Add(itemImage);
            }

            if (!_item.Equals(original))
            {
                _item.UpdatedAt = DateTime.Now;
                await App.DataRepo.UpdateItem(_item);
            }

            FileSystemUtils.ClearTempPath();

            await Shell.Current.GoToAsync($"..?collection={_item.CollectionId.ToString()}&refresh=true");
            Analytics.TrackEvent("EditItem");*/
        }
    }
}