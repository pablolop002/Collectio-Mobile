using Collectio.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [QueryProperty("Collection", "collection")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CollectionEditView : ContentPage
    {
        public string Collection
        {
            set => ((CollectionEditViewModel) BindingContext).Collection = App.DataRepo.GetCollection(value);
        }

        public CollectionEditView()
        {
            InitializeComponent();
        }
    }
}