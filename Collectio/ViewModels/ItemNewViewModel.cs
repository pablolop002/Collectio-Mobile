using System.Threading.Tasks;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using Plugin.StoreReview;
using Xamarin.Essentials;

namespace Collectio.ViewModels
{
    public class ItemNewViewModel : BaseViewModel
    {
        private static int NewItemUsage
        {
            get => Preferences.Get(nameof(NewItemUsage), 0);
            set => Preferences.Set(nameof(NewItemUsage), value);
        }

        public ICommand SaveItemCommand;

        public ItemNewViewModel()
        {
            SaveItemCommand = new AsyncCommand(SaveItem);
        }

        private async Task SaveItem()
        {
            if (NewItemUsage++ == 25) await CrossStoreReview.Current.RequestReview(false);
        }
    }
}