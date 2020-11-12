using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Collectio.Views
{
    [QueryProperty("Collection", "collection")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewItemPage : ContentPage
    {
        private string _collectionId;
        
        public string Collection
        {
            set => _collectionId = Uri.UnescapeDataString(value);
        }

        public NewItemPage()
        {
            InitializeComponent();
        }
    }
}