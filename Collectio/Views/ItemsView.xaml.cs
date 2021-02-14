using System;
using Collectio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [QueryProperty("Collection", "collection")]
    [QueryProperty("Refresh", "refresh")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemsView : ContentPage
    {
        public string Collection
        {
            set => ((ItemsViewModel) BindingContext).Collection =
                App.DataRepo.GetCollection(Uri.UnescapeDataString(value));
        }

        public string Refresh
        {
            set => ((ItemsViewModel) BindingContext).IsRefreshing = value.Equals("true");
        }

        public ItemsView()
        {
            InitializeComponent();
        }
    }
}