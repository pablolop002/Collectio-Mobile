using System.Threading.Tasks;
using System.Windows.Input;
using Collectio.Models;
using Collectio.Resources.Culture;
using Collectio.Utils;
using MvvmHelpers;
using MvvmHelpers.Commands;

namespace Collectio.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private bool _isRefreshing;
        private Collection _collection;
        private Item _selectedItem;

        public ObservableRangeCollection<Item> Items { get; private set; }

        public ICommand RefreshCommand { get; }

        public ICommand AddCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand DuplicateCommand { get; }

        public ICommand EditCommand { get; }

        public Collection Collection
        {
            get => _collection;
            set
            {
                SetProperty(ref _collection, value);
                if (value == null) return;
                Title = value.Name;
                IsRefreshing = true;
            }
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (value != null)
                {
                    Xamarin.Forms.Shell.Current.GoToAsync($"item?item={value.Id.ToString()}");
                    value = null;
                }

                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }

        public ItemsViewModel()
        {
            Items = new ObservableRangeCollection<Item>();
            AddCommand = new AsyncCommand(Add);
            DeleteCommand = new AsyncCommand<Item>(Delete);
            DuplicateCommand = new Command<Item>(Duplicate);
            EditCommand = new Command<Item>(Edit);
            RefreshCommand = new Command(RefreshEvent);
        }

        private void RefreshEvent()
        {
            IsRefreshing = true;

            Items.Clear();
            Items.AddRange(App.DataRepo.GetAllItemsFromCategory(Collection.Id.ToString(), true));

            IsRefreshing = false;
        }

        private async Task Add()
        {
            var answer = await Xamarin.Forms.Shell.Current.DisplayActionSheet(Strings.NewItem, Strings.Cancel, null,
                Strings.Create, Strings.Import, Strings.Duplicate);

            if (answer == null || answer == Strings.Cancel) return;

            if (answer == Strings.Create)
            {
                await Xamarin.Forms.Shell.Current.GoToAsync($"newItem?collection={Collection.Id.ToString()}");
            }
            else if (answer == Strings.Import)
            {
                await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Import, "Próximamente", Strings.Ok);
                //await Shell.Current.GoToAsync($"importItem?collection={Collection.Id.ToString()}");
            }
            else
            {
                await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Duplicate, Strings.DuplicateMessage, Strings.Ok);
            }
        }

        private static void Edit(Item item)
        {
            Xamarin.Forms.Shell.Current.GoToAsync($"editItem?item={item.Id.ToString()}");
        }

        private static void Duplicate(Item item)
        {
            Xamarin.Forms.Shell.Current.GoToAsync(
                $"newItem?collection={item.CollectionId.ToString()}&copyFrom={item.Id.ToString()}");
        }

        private async Task Delete(Item item)
        {
            var aux = await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.SureQuestion, Strings.DeleteItem,
                Strings.Confirm, Strings.Cancel);
            if (!aux) return;

            App.DataRepo.RemoveItem(item.Id.ToString());
            FileSystemUtils.DeleteItem(item.CollectionId.ToString(), item.Id.ToString());

            IsRefreshing = true;
        }
    }
}