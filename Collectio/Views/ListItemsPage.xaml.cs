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
    [QueryProperty("Collection", "collection")]
    [QueryProperty("Refresh", "refresh")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListItemsPage : ContentPage
    {
        private string _collectionId;

        public string Collection
        {
            set
            {
                _collectionId = value;
                BindingContext = new ItemsViewModel(App.DataRepo.GetCollection(Uri.UnescapeDataString(value)));
            }
        }

        public string Refresh
        {
            set => MainThread.BeginInvokeOnMainThread(() => RefreshItemsView.IsRefreshing = value.Equals("true"));
        }

        public ListItemsPage()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (DeviceInfo.Idiom != DeviceIdiom.Tablet) return;
                ItemsView.ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)
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
                ItemsView.SelectedItems = null;
                ItemsView.SelectedItem = null;
            });
        }

        private void ItemsView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is Item item)
            {
                Shell.Current.GoToAsync($"item?item={item.Id.ToString()}");
            }
        }

        private async void Add_OnClicked(object sender, EventArgs e)
        {
            var answer = await Shell.Current.DisplayActionSheet(Strings.NewItem, Strings.Cancel, null,
                Strings.Create, Strings.Import, Strings.Duplicate);

            if (answer == null || answer == Strings.Cancel) return;

            if (answer == Strings.Create)
            {
                await Shell.Current.GoToAsync($"newItem?collection={_collectionId}");
            }
            else if (answer == Strings.Import)
            {
                await Shell.Current.DisplayAlert(Strings.Import, "Próximamente", Strings.Ok);
                //await Shell.Current.GoToAsync($"importItem?collection={_collectionId}");
            }
            else
            {
                await Shell.Current.DisplayAlert(Strings.Duplicate, Strings.DuplicateMessage, Strings.Ok);
            }
        }

        private void Edit_Invoked(object sender, EventArgs eventArgs)
        {
            if (!(((SwipeItemView) sender).BindingContext is Item item)) return;

            Shell.Current.GoToAsync($"editItem?item={item.Id.ToString()}");
        }

        private void Duplicate_Invoked(object sender, EventArgs eventArgs)
        {
            if (!(((SwipeItemView) sender).BindingContext is Item item)) return;

            Shell.Current.GoToAsync($"newItem?collection={item.CollectionId.ToString()}&copyFrom={item.Id.ToString()}");
        }

        private async void Delete_Invoked(object sender, EventArgs eventArgs)
        {
            if (!(((SwipeItemView) sender).BindingContext is Item item)) return;
            var aux = await Shell.Current.DisplayAlert(Strings.SureQuestion, Strings.DeleteItem,
                Strings.Confirm, Strings.Cancel);
            if (!aux) return;

            App.DataRepo.RemoveItem(item.Id.ToString());
            FileSystemUtils.DeleteItem(item.CollectionId.ToString(), item.Id.ToString());

            MainThread.BeginInvokeOnMainThread(() => RefreshItemsView.IsRefreshing = true);
        }
    }
}