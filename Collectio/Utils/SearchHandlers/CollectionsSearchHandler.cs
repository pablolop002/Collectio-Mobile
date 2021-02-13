using System.Linq;
using System.Threading.Tasks;
using Collectio.Models;
using Xamarin.Forms;

namespace Collectio.Utils.SearchHandlers
{
    public class CollectionsSearchHandler : SearchHandler
    {
        protected override void OnQueryChanged(string oldValue, string newValue)
        {
            base.OnQueryChanged(oldValue, newValue);

            if (string.IsNullOrWhiteSpace(newValue))
            {
                ItemsSource = null;
            }
            else
            {
                ItemsSource = App.DataRepo.GetAllCollections()
                    .Where(collection => collection.Name.ToLower().Contains(newValue.ToLower())).ToList();
            }
        }

        protected override async void OnItemSelected(object item)
        {
            base.OnItemSelected(item);
            await Task.Delay(1000);

            await Shell.Current.GoToAsync($"items?collection={((Collection)item).Id}");
        }
    }
}