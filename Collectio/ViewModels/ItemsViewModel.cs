using Collectio.Models;
using MvvmHelpers;
using MvvmHelpers.Commands;

namespace Collectio.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private bool _isRefreshing;
        private Collection _collection;

        public ObservableRangeCollection<Item> Items { get; private set; }
        
        public Command RefreshCommand { get; set; }

        public Collection Collection
        {
            get => _collection;
            set
            {
                if (value != null)
                {
                    Title = value.Name;
                }
                SetProperty(ref _collection, value);
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
            RefreshCommand = new Command(RefreshEvent);
            IsRefreshing = true;
        }

        public ItemsViewModel(Collection collection)
        {
            Collection = collection;
            Items = new ObservableRangeCollection<Item>();
            RefreshCommand = new Command(RefreshEvent);
            IsRefreshing = true;
        }

        private void RefreshEvent()
        {
            IsRefreshing = true;

            Items.Clear();
            Items.AddRange(App.DataRepo.GetAllItemsFromCategory(Collection.Id.ToString(), true));

            IsRefreshing = false;
        }
    }
}