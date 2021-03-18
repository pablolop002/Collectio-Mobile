using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Collectio.Models;
using Collectio.Resources.Culture;
using Collectio.Utils;
using Microsoft.AppCenter.Analytics;
using MvvmHelpers;
using MvvmHelpers.Commands;
using Plugin.StoreReview;
using Xamarin.Essentials;

namespace Collectio.ViewModels
{
    public class CollectionNewViewModel : BaseViewModel
    {
        private IEnumerable<Category> _categories;
        private Category _selectedCategory;
        private Collection _collection;
        private bool _categorySelection = true;
        private bool _collectionDetails;
        private string _imageName = string.Empty;
        private string _file = string.Empty;

        public IEnumerable<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        private static int NewCollectionUsage
        {
            get => Preferences.Get(nameof(NewCollectionUsage), 0);
            set => Preferences.Set(nameof(NewCollectionUsage), value);
        }

        public Collection Collection
        {
            get => _collection;
            set => SetProperty(ref _collection, value);
        }

        public bool CollectionDetails
        {
            get => _collectionDetails;
            set
            {
                if (SetProperty(ref _collectionDetails, value))
                    CategorySelection = !_collectionDetails;
            }
        }

        public bool CategorySelection
        {
            get => _categorySelection;
            set
            {
                if (SetProperty(ref _categorySelection, value))
                    CollectionDetails = !_categorySelection;
            }
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                SetProperty(ref _selectedCategory, value);
                _collection.CategoryId = value.Id;
                CategorySelection = false;
            }
        }

        public string File
        {
            get => _file;
            set => SetProperty(ref _file, value);
        }

        public ICommand SaveItemCommand { get; }

        public ICommand SelectImageCommand { get; }

        public CollectionNewViewModel()
        {
            SaveItemCommand = new AsyncCommand(SaveItem);
            SelectImageCommand = new AsyncCommand(SelectImage);
            Collection = new Collection();
            Categories = App.DataRepo.GetCategories();
        }

        private async Task SelectImage()
        {
            var selection = await Xamarin.Forms.Shell.Current.DisplayActionSheet(Strings.ImageOrigin, Strings.Cancel,
                null,
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
                                File = FileSystemUtils.TempSave(memStream, photo.FileName);
                                _imageName = photo.FileName;
                            }
                        }
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
                                File = FileSystemUtils.TempSave(memStream, photo.FileName);
                                _imageName = photo.FileName;
                            }
                        }
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
            if (SelectedCategory == null)
            {
                await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, Strings.CategoryNotSelected,
                    Strings.Ok);
                return;
            }

            if (string.IsNullOrWhiteSpace(Collection.Name))
            {
                await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, Strings.EmptyCollectionName, Strings.Ok);
                return;
            }

            _collection.CreatedAt = DateTime.Now;
            _collection.UpdatedAt = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(_imageName))
            {
                _collection.Image = _imageName;
            }

            App.DataRepo.AddCollection(ref _collection);

            if (!string.IsNullOrWhiteSpace(_imageName))
            {
                FileSystemUtils.SaveFileFromPath(_file, _imageName, _collection.Id);
                FileSystemUtils.ClearTempPath();
            }

            if (NewCollectionUsage++ == 5) await CrossStoreReview.Current.RequestReview(false);

            await Xamarin.Forms.Shell.Current.GoToAsync("..?refresh=true");
            Analytics.TrackEvent("CreateCollection");
        }
    }
}