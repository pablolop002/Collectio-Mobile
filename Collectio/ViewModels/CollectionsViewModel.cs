using System.Collections.ObjectModel;
using Collectio.Models;
using MvvmHelpers;
using MvvmHelpers.Commands;

namespace Collectio.ViewModels
{
    public class CollectionsViewModel : BaseViewModel
    {
        private bool _isRefreshing;
        
        public ObservableCollection<CollectionGroup> CollectionGroups { get; set; }
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

        public CollectionsViewModel()
        {
            CollectionGroups = new ObservableCollection<CollectionGroup>(App.DataRepo.GetCollectionGroups(true));
            RefreshCommand = new Command(RefreshEvent);
        }

        private void RefreshEvent()
        {
            IsRefreshing = true;

            CollectionGroups.Clear();
            foreach (var group in App.DataRepo.GetCollectionGroups(true))
            {
                CollectionGroups.Add(group);
            }

            IsRefreshing = false;
        }
    }
}