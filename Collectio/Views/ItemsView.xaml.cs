using System;
using System.Diagnostics.CodeAnalysis;
using Collectio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [QueryProperty("Collection", "collection")]
    [QueryProperty("Refresh", "refresh")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
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