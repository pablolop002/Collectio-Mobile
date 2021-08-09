using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Collectio.Models;
using Collectio.Resources.Culture;
using Collectio.Utils;
using Collectio.ViewModels.BaseViewModels;
using Microsoft.AppCenter.Analytics;
using MvvmHelpers.Commands;
using Plugin.StoreReview;
using Xamarin.Essentials;

namespace Collectio.ViewModels
{
    public class CollectionNewViewModel : BaseCollectionViewModel
    {
        private Category _selectedCategory;
        private bool _categorySelection = true;
        private bool _collectionDetails;

        private static int NewCollectionUsage
        {
            get => Preferences.Get(nameof(NewCollectionUsage), 0);
            set => Preferences.Set(nameof(NewCollectionUsage), value);
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
                Collection.CategoryId = value.Id;
                CategorySelection = false;
            }
        }

        public ICommand SaveItemCommand { get; }

        public CollectionNewViewModel()
        {
            SaveItemCommand = new AsyncCommand(SaveItem);
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

            Collection.CreatedAt = DateTime.Now;
            Collection.UpdatedAt = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(ImageName))
            {
                Collection.Image = ImageName;
            }

            var id = await App.DataRepo.AddCollection(Collection);

            if (id != -1)
            {
                if (!string.IsNullOrWhiteSpace(ImageName))
                {
                    FileSystemUtils.SaveFileFromPath(File, ImageName, id);
                    FileSystemUtils.ClearTempPath();
                }

                if (NewCollectionUsage++ == 5) await CrossStoreReview.Current.RequestReview(false);

                await Xamarin.Forms.Shell.Current.GoToAsync("..?refresh=true");
            }
            else
            {
                await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, "Strings.NewCollectionError", Strings.Ok);
            }

            Analytics.TrackEvent("CreateCollection");
        }
    }
}