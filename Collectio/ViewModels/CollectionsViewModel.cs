using System.Threading.Tasks;
using System.Windows.Input;
using Collectio.Models;
using Collectio.Resources.Culture;
using Collectio.Utils;
using MvvmHelpers;
using MvvmHelpers.Commands;

namespace Collectio.ViewModels
{
    public class CollectionsViewModel : BaseViewModel
    {
        private Collection _selectedCollection;

        public Collection SelectedCollection
        {
            get => _selectedCollection;
            set
            {
                if (value != null)
                {
                    Xamarin.Forms.Shell.Current.GoToAsync($"items?collection={value.Id.ToString()}");
                    value = null;
                }

                _selectedCollection = value;
                OnPropertyChanged();
            }
        }

        public ObservableRangeCollection<CollectionGroup> CollectionGroups { get; set; }

        public ICommand RefreshCommand { get; set; }

        public ICommand SyncCommand { get; set; }

        public ICommand AddCommand { get; set; }

        public ICommand EditCommand { get; set; }

        public ICommand DeleteCommand { get; set; }

        public CollectionsViewModel()
        {
            CollectionGroups = new ObservableRangeCollection<CollectionGroup>(App.DataRepo.GetCollectionGroups());
            RefreshCommand = new Command(RefreshEvent);
            SyncCommand = new AsyncCommand(Sync);
            AddCommand = new AsyncCommand(Add);
            EditCommand = new Command<Collection>(Edit);
            DeleteCommand = new AsyncCommand<Collection>(Delete);
        }

        private static async Task Add()
        {
            var answer = await Xamarin.Forms.Shell.Current.DisplayActionSheet(Strings.NewCollection, Strings.Cancel,
                null, Strings.Create, Strings.Import);

            if (answer == null || answer == Strings.Cancel) return;

            if (answer == Strings.Create)
            {
                await Xamarin.Forms.Shell.Current.GoToAsync("newCollection");
            }
            else if (answer == Strings.Import)
            {
                await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Import, "Próximamente", Strings.Ok);
                //await Xamarin.Forms.Shell.Current.GoToAsync("importCollection");
            }
        }

        private async Task Sync()
        {
            var sure = await Xamarin.Forms.Shell.Current.DisplayAlert("Strings.Sure", "Strings.SyncOfflineChanges",
                Strings.Ok, Strings.Cancel);

            if (!sure) return;

            var offlineActions = App.DataRepo.GetOfflineActions();
            foreach (var offlineAction in offlineActions)
            {
                switch (offlineAction.Type)
                {
                    case "delete":
                        switch (offlineAction.ElementType)
                        {
                            case "collection":
                                //App.DataRepo.DeleteCollection(int.Parse(offlineAction.ElementIdentifier), offlineAction.Id);
                                break;
                            case "item":
                                //App.DataRepo.DeleteItem(int.Parse(offlineAction.ElementIdentifier), offlineAction.Id);
                                break;
                            case "itemImage":
                                //App.DataRepo.DeleteItemImage(int.Parse(offlineAction.ElementIdentifier), offlineAction.Id);
                                break;
                            default:
                                //App.DataRepo.DeleteApikey(offlineAction.ElementIdentifier, offlineAction.Id);
                                break;
                        }

                        break;
                    case "update":
                        switch (offlineAction.ElementType)
                        {
                            case "collection":
                                //App.DataRepo.UpdateCollection(int.Parse(offlineAction.ElementIdentifier), offlineAction.Id);
                                break;
                            case "item":
                                //App.DataRepo.UpdateItem(int.Parse(offlineAction.ElementIdentifier), offlineAction.Id);
                                break;
                            case "itemImage":
                                //App.DataRepo.UpdateItemImage(int.Parse(offlineAction.ElementIdentifier), offlineAction.Id);
                                break;
                            default:
                                //App.DataRepo.UpdateApikey(offlineAction.ElementIdentifier, offlineAction.Id);
                                break;
                        }

                        break;
                    default:
                        switch (offlineAction.ElementType)
                        {
                            case "collection":
                                //App.DataRepo.AddCollection(int.Parse(offlineAction.ElementIdentifier), offlineAction.Id);
                                break;
                            case "item":
                                //App.DataRepo.AddItem(int.Parse(offlineAction.ElementIdentifier), offlineAction.Id);
                                break;
                            case "itemImage":
                                //App.DataRepo.AddItemImage(int.Parse(offlineAction.ElementIdentifier), offlineAction.Id);
                                break;
                        }

                        break;
                }
            }
        }

        private void Edit(Collection collection)
        {
            Xamarin.Forms.Shell.Current.GoToAsync($"editCollection?collection={collection.Id.ToString()}");
        }

        private async Task Delete(Collection collection)
        {
            var aux = await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.SureQuestion, Strings.DeleteCollection,
                Strings.Confirm, Strings.Cancel);
            if (!aux) return;

            App.DataRepo.RemoveCollection(collection.Id.ToString());
            FileSystemUtils.DeleteCollection(collection.Id.ToString());

            IsBusy = true;
        }

        private void RefreshEvent()
        {
            IsBusy = true;

            CollectionGroups.Clear();
            CollectionGroups.AddRange(App.DataRepo.GetCollectionGroups());

            IsBusy = false;
        }
    }
}