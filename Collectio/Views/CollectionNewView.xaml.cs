using System.Diagnostics.CodeAnalysis;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [SuppressMessage("ReSharper", "RedundantExtendsListEntry")]
    public partial class CollectionNewView : ContentPage
    {
        public CollectionNewView()
        {
            InitializeComponent();
        }
    }
}