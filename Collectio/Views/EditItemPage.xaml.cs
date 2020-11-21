using System;
using Collectio.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [QueryProperty("Item", "item")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditItemPage : ContentPage
    {
        private Item _item;
        
        public string Item
        {
            set
            {
                BindingContext = _item = App.DataRepo.GetItem(value, true);
            }
        }

        public EditItemPage()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
        }

        private void Done_OnClicked(object sender, EventArgs e)
        {
            //ToDo
        }

        private void AddImage_OnClicked(object sender, EventArgs e)
        {
            //ToDo
        }
    }
}