using System.Collections.ObjectModel;
using Collectio.Models;
using MvvmHelpers;
using MvvmHelpers.Commands;

namespace Collectio.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private bool _isRefreshing;
        
        public ObservableCollection<Item> Items { get; private set; }
        public Command RefreshCommand { get; set; }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged();
            }
        }
        public ItemsViewModel(Collection collection)
        {
            Title = collection.Name;
            Items = collection.Items;
            RefreshCommand = new Command(RefreshEvent);
        }

        private void RefreshEvent()
        {
            IsRefreshing = true;

            /*Items.Clear();
            foreach (var group in App.DataRepo.GetItems(true))
            {
                Items.Add(group);
            }*/

            IsRefreshing = false;
        }
    }
}