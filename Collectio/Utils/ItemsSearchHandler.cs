using System.Linq;
using System.Threading.Tasks;
using Collectio.Models;
using Xamarin.Forms;

namespace Collectio.Utils
{
    public class ItemsSearchHandler : SearchHandler
    {
        public static readonly BindableProperty CollectionProperty =
            BindableProperty.Create(nameof(Collection), typeof(int), typeof(SearchHandler), null,
                BindingMode.OneTime);
        public int Collection
        {
            get => (int) GetValue(CollectionProperty);
            set => SetValue(CollectionProperty, value);
        }

        protected override void OnQueryChanged(string oldValue, string newValue)
        {
            base.OnQueryChanged(oldValue, newValue);

            if (string.IsNullOrWhiteSpace(newValue))
            {
                ItemsSource = null;
            }
            else
            {
                ItemsSource = App.DataRepo.GetAllItemsFromCategory(Collection.ToString())
                    .Where(item => item.Name.ToLower().Contains(newValue.ToLower())).ToList();
            }
        }

        protected override async void OnItemSelected(object item)
        {
            base.OnItemSelected(item);
            await Task.Delay(1000);

            await Shell.Current.GoToAsync($"item?item={((Item) item).Id}");
        }
    }
}