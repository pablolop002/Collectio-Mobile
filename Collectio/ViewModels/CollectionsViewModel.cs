using System.Collections.Generic;
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
        private ObservableRangeCollection<CollectionGroup> _collectionGroups;

        public Collection SelectedCollection
        {
            get => _selectedCollection;
            set
            {
                if (value != null)
                {
                    Xamarin.Forms.Shell.Current.GoToAsync($"items?collection={value.Id.ToString()}");
                }

                _selectedCollection = null;
                OnPropertyChanged();
            }
        }

        public ObservableRangeCollection<CollectionGroup> CollectionGroups
        {
            get => _collectionGroups;
            set => SetProperty(ref _collectionGroups, value);
        }

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

                var csv = await Xamarin.Essentials.FilePicker.PickAsync(new Xamarin.Essentials.PickOptions
                {
                    FileTypes = new Xamarin.Essentials.FilePickerFileType(
                        new Dictionary<Xamarin.Essentials.DevicePlatform, IEnumerable<string>>
                        {
                            {
                                Xamarin.Essentials.DevicePlatform.iOS,
                                new[]
                                {
                                    "public.comma-separated-values-text", "public.delimited-values-text",
                                    "public.tab-separated-values-text", "public.utf8-tab-separated-values-text"
                                }
                            }, // or general UTType values
                            {Xamarin.Essentials.DevicePlatform.Android, new[] {".csv", "text/plain"}}
                        })
                });

                if (csv == null) return;

                using var stream = await csv.OpenReadAsync();
                using var reader = new System.IO.StreamReader(stream);
                using var csvReader =
                    new CsvHelper.CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture);
                var collection = csvReader.GetRecord<Collection>();

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
                                await App.DataRepo.RemoveCollection(offlineAction.ElementIdentifier, offlineAction.Id);
                                break;
                            case "item":
                                await App.DataRepo.RemoveItem(offlineAction.ElementIdentifier, offlineAction.Id);
                                break;
                            case "itemImage":
                                await App.DataRepo.RemoveItemImage(offlineAction.ElementIdentifier, offlineAction.Id);
                                break;
                            default:
                                await App.DataRepo.RemoveApikey(offlineAction.ElementIdentifier, offlineAction.Id);
                                break;
                        }

                        break;
                    case "update":
                        switch (offlineAction.ElementType)
                        {
                            case "collection":
                                App.DataRepo.UpdateCollection(
                                    App.DataRepo.GetCollection(offlineAction.ElementIdentifier), offlineAction.Id);
                                break;
                            case "item":
                                App.DataRepo.UpdateItem(App.DataRepo.GetItem(offlineAction.ElementIdentifier),
                                    offlineAction.Id);
                                break;
                            case "itemImage":
                                App.DataRepo.UpdateItemImage(App.DataRepo.GetItemImage(offlineAction.ElementIdentifier),
                                    offlineAction.Id);
                                break;
                            case "user":
                                await App.DataRepo.UpdateUser(await App.DataRepo.GetUser(offlineAction.Id, false),
                                    offlineAction.Id);
                                break;
                            default:
                                await App.DataRepo.UpdateApikey(App.DataRepo.GetApikey(offlineAction.ElementIdentifier),
                                    offlineAction.Id);
                                break;
                        }

                        break;
                    default:
                        switch (offlineAction.ElementType)
                        {
                            case "collection":
                                App.DataRepo.AddCollectionServer(
                                    App.DataRepo.GetCollection(offlineAction.ElementIdentifier), offlineAction.Id);
                                break;
                            case "item":
                                //App.DataRepo.AddItemServer(App.DataRepo.GetItem(offlineAction.ElementIdentifier), offlineAction.Id);
                                break;
                            case "itemImage":
                                //App.DataRepo.AddItemImageServer(App.DataRepo.GetItemImage(offlineAction.ElementIdentifier), offlineAction.Id);
                                break;
                        }

                        break;
                }
            }
        }

        private static void Edit(Collection collection)
        {
            Xamarin.Forms.Shell.Current.GoToAsync($"editCollection?collection={collection.Id.ToString()}");
        }

        private async Task Delete(Collection collection)
        {
            var aux = await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.SureQuestion, Strings.DeleteCollection,
                Strings.Confirm, Strings.Cancel);
            if (!aux) return;

            await App.DataRepo.RemoveCollection(collection.Id.ToString());
            FileSystemUtils.DeleteCollection(collection.Id.ToString());

            IsBusy = true;
        }

        private void RefreshEvent()
        {
            IsBusy = true;

            CollectionGroups.Clear();
            //CollectionGroups.AddRange(App.DataRepo.GetCollectionGroups());
            CollectionGroups = new ObservableRangeCollection<CollectionGroup>(App.DataRepo.GetCollectionGroups());

            IsBusy = false;
        }
    }
}