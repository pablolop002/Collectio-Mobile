using System.Diagnostics.CodeAnalysis;
using Collectio.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            
            Routing.RegisterRoute("settings", typeof(SettingsPage));
            Routing.RegisterRoute("comments", typeof(CommentsPage));
            
            Routing.RegisterRoute("newCollection", typeof(NewCollectionPage));
            Routing.RegisterRoute("editCollection", typeof(EditCollectionPage));
            
            Routing.RegisterRoute("items", typeof(ListItemsPage));
            Routing.RegisterRoute("item", typeof(DetailItemPage));
            Routing.RegisterRoute("newItem", typeof(NewItemPage));
            Routing.RegisterRoute("editItem", typeof(EditItemPage));
        }
    }
}