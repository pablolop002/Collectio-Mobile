using System.Diagnostics.CodeAnalysis;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [QueryProperty("Item", "item")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
    public partial class ItemDetailView : ContentPage
    {
        public string Item
        {
            set => BindingContext = App.DataRepo.GetItem(value, true);
        }

        public ItemDetailView()
        {
            InitializeComponent();
        }
    }
}