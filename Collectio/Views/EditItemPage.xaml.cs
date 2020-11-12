using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [QueryProperty("Item", "item")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditItemPage : ContentPage
    {
        private string _itemId;
        
        public string Item
        {
            set
            {
                _itemId = Uri.UnescapeDataString(value);
                BindingContext = App.DataRepo.GetItem(_itemId);
            }
        }

        public EditItemPage()
        {
            InitializeComponent();
        }
    }
}