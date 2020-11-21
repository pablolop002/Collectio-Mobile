using System;
using System.Linq;
using Collectio.Models;
using Collectio.Resources.Culture;
using Collectio.Utils;
using Collectio.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [QueryProperty("Refresh", "refresh")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListCollectionsPage : ContentPage
    {
        public string Refresh
        {
            set => MainThread.BeginInvokeOnMainThread(() => RefreshCollectionView.IsRefreshing = value.Equals("true"));
        }
        
        public ListCollectionsPage()
        {
            InitializeComponent();
            BindingContext = new CollectionsViewModel();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (DeviceInfo.Idiom != DeviceIdiom.Tablet) return;
                CollectionsCollectionView.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)
                {
                    HorizontalItemSpacing = 10, VerticalItemSpacing = 10
                };
            });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CollectionsCollectionView.SelectedItems = null;
                CollectionsCollectionView.SelectedItem = null;
            });
        }

        private void CollectionView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is Collection collection)
            {
                Shell.Current.GoToAsync($"items?collection={collection.Id.ToString()}");
            }
        }

        private async void Add_OnClicked(object sender, EventArgs e)
        {
            var answer = await Shell.Current.DisplayActionSheet(Strings.NewCollection, Strings.Cancel, null,
                Strings.Create, Strings.Import);

            if (answer == null || answer == Strings.Cancel) return;

            if (answer == Strings.Create)
            {
                await Shell.Current.GoToAsync("newCollection");
            }
            else if (answer == Strings.Import)
            {
                await Shell.Current.DisplayAlert("", "Import", "OK");
                //await Shell.Current.GoToAsync("importCollection");
            }
        }

        private void Edit_Invoked(object sender, EventArgs eventArgs)
        {
            if (!(((SwipeItemView) sender).BindingContext is Collection collection)) return;
            
            Shell.Current.GoToAsync($"editCollection?collection={collection.Id.ToString()}");
        }

        private async void Delete_Invoked(object sender, EventArgs eventArgs)
        {
            if (!(((SwipeItemView) sender).BindingContext is Collection collection)) return;
            var aux = await Shell.Current.DisplayAlert(Strings.SureQuestion, Strings.DeleteCollection,
                Strings.Confirm, Strings.Cancel);
            if (!aux) return;
            
            App.DataRepo.RemoveCollection(collection.Id.ToString());
            FileSystemUtils.DeleteCollection(collection.Id.ToString());
            
            MainThread.BeginInvokeOnMainThread(() => RefreshCollectionView.IsRefreshing = true);
        }
    }
}