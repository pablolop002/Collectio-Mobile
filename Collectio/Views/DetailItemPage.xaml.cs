using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [QueryProperty("Item", "item")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DetailItemPage : ContentPage
    {
        public string Item
        {
            set
            {
                var item = App.DataRepo.GetItem(value, true);
                BindingContext = item;
                Subcategory.Text = App.DataRepo.GetSubcategory(item.SubcategoryId.ToString()).Name;
            }
        }

        public DetailItemPage()
        {
            InitializeComponent();
            Shell.SetTabBarIsVisible(this, false);
        }
    }
}