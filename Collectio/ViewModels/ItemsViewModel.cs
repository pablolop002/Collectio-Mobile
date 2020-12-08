using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        public int CollectionId { get; }

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
            CollectionId = collection.Id;
            Items = new ObservableCollection<Item>();
            RefreshCommand = new Command(RefreshEvent);
            IsRefreshing = true;
        }

        private void RefreshEvent()
        {
            IsRefreshing = true;

            Items.Clear();
            foreach (var item in App.DataRepo.GetAllItemsFromCategory(CollectionId.ToString(), true))
            {
                Items.Add(item);
            }

            IsRefreshing = false;
        }
    }
}