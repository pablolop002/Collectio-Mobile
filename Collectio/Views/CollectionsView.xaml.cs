using System.Diagnostics.CodeAnalysis;
using Collectio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [QueryProperty("Refresh", "refresh")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
    public partial class CollectionsView : ContentPage
    {
        public string Refresh
        {
            set => ((CollectionsViewModel) BindingContext).IsBusy = value.Equals("true");
        }
        
        public CollectionsView()
        {
            InitializeComponent();
        }
    }
}