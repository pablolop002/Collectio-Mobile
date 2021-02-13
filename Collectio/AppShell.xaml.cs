using System.Diagnostics.CodeAnalysis;
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
            
            Routing.RegisterRoute("settings", typeof(Views.SettingsView));
            Routing.RegisterRoute("comments", typeof(Views.CommentsView));
            
            Routing.RegisterRoute("newCollection", typeof(Views.CollectionNewView));
            Routing.RegisterRoute("editCollection", typeof(Views.CollectionEditView));
            
            Routing.RegisterRoute("items", typeof(Views.ItemsView));
            Routing.RegisterRoute("item", typeof(Views.ItemDetailView));
            Routing.RegisterRoute("newItem", typeof(Views.ItemNewView));
            Routing.RegisterRoute("editItem", typeof(Views.ItemEditView));
        }
    }
}