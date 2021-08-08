using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Collectio.Models;
using Collectio.Utils;
using MvvmHelpers;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Extensions;
using Xamarin.Essentials;

namespace Collectio.Repositories
{
    public class DataRepository
    {
        private static DateTime LastSyncedUser
        {
            get => Preferences.Get("lastSyncedUser", DateTime.MinValue);
            set => Preferences.Set("lastSyncedUser", value);
        }

        private static DateTime LastSyncedApiKeys
        {
            get => Preferences.Get("lastSyncedApiKeys", DateTime.MinValue);
            set => Preferences.Set("lastSyncedApiKeys", value);
        }

        private static DateTime LastSyncedCollections
        {
            get => Preferences.Get("lastSyncedCollections", DateTime.MinValue);
            set => Preferences.Set("lastSyncedCollections", value);
        }

        private static bool LoggedIn => Preferences.Get("LoggedIn", false);

        private SQLiteConnection _database;
        public readonly RestServiceUtils RestService;
        private readonly string _databasePath;

        public DataRepository()
        {
            try
            {
                _databasePath = System.IO.Path.Combine(FileSystem.AppDataDirectory, "collectio.db");
                _database = new SQLiteConnection(_databasePath);
                RestService = new RestServiceUtils();
                _database.CreateTable<User>();
                _database.CreateTable<Apikey>();
                _database.CreateTable<Category>();
                _database.CreateTable<Subcategory>();
                _database.CreateTable<Collection>();
                _database.CreateTable<Item>();
                _database.CreateTable<ItemImage>();
                _database.CreateTable<OfflineActions>();

                if (!Preferences.Get("LoggedIn", false)) CreateUser();
                Triggers();
                CreateOrUpdateCategories();
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "Database Creation");
            }
        }

        #region Triggers

        private void Triggers()
        {
            _database.Execute(
                "create trigger update_items_on_itemsImage_insert after insert on ItemImages for each row update Items set UpdatedAt = CURRENT_TIMESTAMP WHERE Id = new.ItemId;");
            _database.Execute(
                "create trigger update_items_on_itemsImage_update after update on ItemImages for each row update Items set UpdatedAt = CURRENT_TIMESTAMP WHERE Id = new.ItemId;");
            _database.Execute(
                "create trigger update_items_on_itemsImage_delete after delete on ItemImages for each row update Items set UpdatedAt = CURRENT_TIMESTAMP WHERE Id = old.ItemId;");
            _database.Execute(
                "create trigger update_collections_on_items_insert after insert on Items for each row update Collections set UpdatedAt = CURRENT_TIMESTAMP WHERE Id = new.CollectionId;");
            _database.Execute(
                "create trigger update_collections_on_items_update after update on Items for each row update Collections set UpdatedAt = CURRENT_TIMESTAMP WHERE Id = new.CollectionId;");
            _database.Execute(
                "create trigger update_collections_on_items_delete after delete on Items for each row update Collections set UpdatedAt = CURRENT_TIMESTAMP WHERE Id = old.CollectionId;");
        }

        #endregion

        #region Backup and Delete

        public bool CreateBackup()
        {
            _database.Close();

            var ret = FileSystemUtils.BackupDataAndDatabase(_databasePath);

            _database = new SQLiteConnection(_databasePath);

            return ret;
        }

        public bool RestoreBackup()
        {
            _database.Close();

            var ret = FileSystemUtils.RestoreBackupDataAndDatabase(_databasePath);

            _database = new SQLiteConnection(_databasePath);

            return ret;
        }

        public bool DeleteAllData(bool withUser = false)
        {
            _database.DropTable<ItemImage>();
            _database.DropTable<Item>();
            _database.DropTable<Collection>();
            _database.CreateTable<Collection>();
            _database.CreateTable<Item>();
            _database.CreateTable<ItemImage>();
            if (withUser)
            {
                CreateUser();
            }

            var ret = FileSystemUtils.DeleteAllData();

            return ret;
        }

        #endregion

        #region Categories and CollectionGroups

        private async void CreateOrUpdateCategories()
        {
            var aux = await RestService.GetRequest("/categories");
            var response = JsonConvert.DeserializeObject<ResponseWs<IEnumerable<Category>>>(aux);

            if (response.Status.Equals("ok"))
            {
                foreach (var category in response.Data)
                {
                    _database.InsertOrReplace(category);
                }
            }
            else
            {
                await Xamarin.Forms.Shell.Current.DisplayAlert(Resources.Culture.Strings.Error,
                    response.Message, Resources.Culture.Strings.Ok);
            }

            CreateOrUpdateSubcategories();
        }

        public IEnumerable<Category> GetCategories()
        {
            return _database.Table<Category>().ToList();
        }

        public IEnumerable<CollectionGroup> GetCollectionGroups()
        {
            var ret = new ObservableCollection<CollectionGroup>();

            var groups = GetCategories();

            foreach (var collectionGroup in from category in groups
                let collections =
                    new ObservableRangeCollection<Collection>(GetCollectionsByGroupId(category.Id.ToString()))
                where collections.Count > 0
                select new CollectionGroup(category.Name, collections)
                {
                    Id = category.Id
                })
            {
                ret.Add(collectionGroup);
            }

            return ret;
        }

        #endregion

        #region Subcategories

        private async void CreateOrUpdateSubcategories()
        {
            var aux = await RestService.GetRequest("/subcategories");
            var response = JsonConvert.DeserializeObject<ResponseWs<IEnumerable<Subcategory>>>(aux);

            if (response.Status.Equals("ok"))
            {
                foreach (var subcategory in response.Data)
                {
                    _database.InsertOrReplace(subcategory);
                }
            }
            else
            {
                await Xamarin.Forms.Shell.Current.DisplayAlert(Resources.Culture.Strings.Error,
                    response.Message, Resources.Culture.Strings.Ok);
            }
        }

        public IEnumerable<Subcategory> GetSubcategoriesByCategoryId(string categoryId)
        {
            return _database.Query<Subcategory>("Select * from Subcategory Where CategoryId = ?", categoryId);
        }

        public Subcategory GetSubcategory(string id)
        {
            return _database.Get<Subcategory>(id);
        }

        #endregion

        public async Task UploadAllData()
        {
            var collections = _database.GetAllWithChildren<Collection>(collection => true, true);
            foreach (var collection in collections)
            {
                var form = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    JsonConvert.SerializeObject(collection));
                var aux = await RestService.PutRequest("/collections", form, new[] {new FileResult(collection.File)});
                var response = JsonConvert.DeserializeObject<ResponseWs<int>>(aux);
                if (response.Status.Equals("ok"))
                {
                    collection.ServerId = response.Data;
                    _database.Update(collection);

                    foreach (var item in collection.Items)
                    {
                        item.CollectionId = response.Data;
                        var itemForm = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                            JsonConvert.SerializeObject(item));
                        var itemAux =
                            await RestService.PutRequest("/items", itemForm,
                                item.Images.Select(itemImage => new FileResult(itemImage.File)).ToArray());
                        var itemResponse = JsonConvert.DeserializeObject<ResponseWs<Dictionary<string, int>>>(itemAux);
                        if (itemResponse.Status.Equals("ok"))
                        {
                            foreach (var ids in itemResponse.Data)
                            {
                                if (ids.Key.Equals("item"))
                                {
                                    item.ServerId = ids.Value;
                                    _database.Update(item);
                                }
                                else
                                {
                                    var itemImage = item.Images.Find(e => e.Image.Equals(ids.Key));
                                    itemImage.ServerId = ids.Value;
                                    _database.Update(itemImage);
                                }
                            }
                        }
                        else
                        {
                            _database.Insert(new OfflineActions()
                            {
                                Type = "insert",
                                ElementType = "item",
                                ElementIdentifier = item.Id.ToString()
                            });
                        }
                    }
                }
                else
                {
                    _database.Insert(new OfflineActions()
                    {
                        Type = "insert",
                        ElementType = "collection",
                        ElementIdentifier = collection.Id.ToString()
                    });
                }
            }
        }

        #region Collection

        public Collection GetCollection(string id, bool withChildren = false)
        {
            return withChildren ? _database.GetWithChildren<Collection>(id) : _database.Get<Collection>(id);
        }

        public IEnumerable<Collection> GetAllCollections()
        {
            return _database.GetAllWithChildren<Collection>(c => true);
        }

        public void AddCollection(ref Collection collection, int? offlineAction = null)
        {
            _database.Insert(collection);

            if (!LoggedIn) return;
            AddCollectionServer(collection, offlineAction);
        }

        public async void AddCollectionServer(Collection collection, int? offlineAction)
        {
            var form = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(collection));
            var files = collection.Image == null ? null : new[] {new FileResult(collection.TempFile)};
            var aux = await RestService.PutRequest("/collections", form, files);
            var response = JsonConvert.DeserializeObject<ResponseWs<int?>>(aux);
            if (response.Status.Equals("ok"))
            {
                collection.ServerId = response.Data;
                _database.Update(collection);

                if (offlineAction != null)
                {
                    _database.Delete<OfflineActions>(offlineAction);
                }
            }
            else
            {
                if (offlineAction != null) return;
                _database.Insert(new OfflineActions()
                {
                    ElementIdentifier = collection.Id.ToString(),
                    ElementType = "collection",
                    Type = "insert"
                });
            }
        }

        public async void UpdateCollection(Collection collection, int? offlineAction = null)
        {
            var oldCollection = _database.Get<Collection>(collection.Id);

            if (offlineAction == null)
            {
                _database.Update(collection);
            }

            if (!LoggedIn || collection.ServerId == null) return;

            var form = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(collection));
            var files = collection.Image == oldCollection.Image
                        || offlineAction != null && !_database.Get<OfflineActions>(offlineAction).ImageUpdated
                ? null
                : new[] {new FileResult(collection.File)};
            var aux = await RestService.PostRequest("/collections", form, files);
            var response = JsonConvert.DeserializeObject<ResponseWs<int?>>(aux);

            if (response.Status.Equals("ok"))
            {
                if (offlineAction != null)
                {
                    _database.Delete<OfflineActions>(offlineAction);
                }
            }
            else
            {
                if (offlineAction != null) return;
                _database.Insert(new OfflineActions
                {
                    ElementIdentifier = collection.Id.ToString(),
                    ElementType = "collection",
                    Type = "update"
                });
            }
        }

        public async Task RemoveCollection(string id, int? offlineAction = null)
        {
            var collection = GetCollection(id, true);

            if (collection != null)
            {
                foreach (var item in collection.Items)
                {
                    foreach (var itemImage in item.Images)
                    {
                        _database.Delete(itemImage);
                    }

                    _database.Delete(item);
                }

                _database.Delete(collection);
            }

            if (!LoggedIn || offlineAction == null && collection?.ServerId == null) return;

            string aux;
            if (offlineAction == null)
            {
                aux = await RestService.DeleteRequest($"/collections/{collection.ServerId.ToString()}");
            }
            else
            {
                var action = _database.Get<OfflineActions>(offlineAction);
                aux = await RestService.DeleteRequest($"/collections/{action.ElementIdentifier}");
            }

            var response = JsonConvert.DeserializeObject<ResponseWs<object>>(aux);

            if (response.Status.Equals("ok"))
            {
                if (offlineAction == null) return;
                _database.Delete<OfflineActions>(offlineAction);
            }
            else
            {
                if (offlineAction != null) return;
                _database.Insert(new OfflineActions()
                {
                    ElementIdentifier = collection.ServerId.ToString(),
                    ElementType = "collection",
                    Type = "delete"
                });
            }
        }

        public IEnumerable<Collection> GetCollectionsByGroupId(string id)
        {
            return _database.Query<Collection>("Select * from Collection Where CategoryId = ?", id);
        }

        #endregion

        #region Item

        public Item GetItem(string id, bool withChildren = false)
        {
            return withChildren ? _database.GetWithChildren<Item>(id) : _database.Get<Item>(id);
        }

        public IEnumerable<Item> GetAllItemsFromCategory(string collectionId, bool withChildren = false)
        {
            if (withChildren)
            {
                var collId = int.Parse(collectionId);
                return _database.GetAllWithChildren<Item>(item => item.CollectionId == collId);
            }
            else
            {
                return _database.Query<Item>("Select * From Item WHERE CollectionId = ?", collectionId);
            }
        }

        public void AddItem(ref Item item, bool recursive = false)
        {
            if (recursive)
            {
                _database.InsertWithChildren(item, true);
            }
            else
            {
                _database.Insert(item);
            }
        }

        public async void UpdateItem(Item item, int? offlineAction = null)
        {
            if (offlineAction == null)
            {
                _database.Update(item);
            }

            if (!LoggedIn || item.ServerId == null) return;

            var form = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                JsonConvert.SerializeObject(item));
            //form.Add("CollectionServerId", _database.Get<Collection>(item.CollectionId).ServerId.ToString());
            var files = offlineAction != null && !_database.Get<OfflineActions>(offlineAction).ImageUpdated
                ? null
                : new[] {new FileResult(item.File)};
            var aux = await RestService.PostRequest("/items", form, files);
            var response = JsonConvert.DeserializeObject<ResponseWs<int?>>(aux);

            if (response.Status.Equals("ok"))
            {
                if (offlineAction != null)
                {
                    _database.Delete<OfflineActions>(offlineAction);
                }
            }
            else
            {
                if (offlineAction != null) return;
                _database.Insert(new OfflineActions
                {
                    ElementIdentifier = item.Id.ToString(),
                    ElementType = "item",
                    Type = "update"
                });
            }
        }

        public async Task RemoveItem(string id, int? offlineAction = null)
        {
            var item = GetItem(id, true);

            if (item != null)
            {
                foreach (var itemImage in item.Images)
                {
                    _database.Delete(itemImage);
                }

                _database.Delete(item);
            }

            if (!LoggedIn || offlineAction == null && item?.ServerId == null) return;

            string aux;

            if (offlineAction == null)
            {
                aux = await RestService.DeleteRequest($"/items/{item.ServerId.ToString()}");
            }
            else
            {
                var action = _database.Get<OfflineActions>(offlineAction);
                aux = await RestService.DeleteRequest($"/items/{action.ElementIdentifier}");
            }

            var response = JsonConvert.DeserializeObject<ResponseWs<object>>(aux);

            if (response.Status.Equals("ok"))
            {
                if (offlineAction == null) return;
                _database.Delete<OfflineActions>(offlineAction);
            }
            else
            {
                if (offlineAction != null) return;
                _database.Insert(new OfflineActions
                {
                    ElementIdentifier = item.ServerId.ToString(),
                    ElementType = "item",
                    Type = "delete"
                });
            }
        }

        #endregion

        #region ItemImages

        public ItemImage GetItemImage(string id)
        {
            return _database.Get<ItemImage>(id);
        }

        public void AddItemImage(ItemImage itemImage)
        {
            _database.Insert(itemImage);
        }

        public void UpdateItemImage(ItemImage itemImage, int? offlineAction = null)
        {
            if (offlineAction == null)
            {
                _database.Update(itemImage);
            }
        }

        public async Task RemoveItemImage(string id, int? offlineAction = null)
        {
            var itemImage = GetItemImage(id);

            if (itemImage != null)
            {
                _database.Delete(itemImage);
            }

            if (!LoggedIn || offlineAction == null && itemImage?.ServerId == null) return;

            string aux;

            if (offlineAction == null)
            {
                aux = await RestService.DeleteRequest($"/items/images/{itemImage.ServerId.ToString()}");
            }
            else
            {
                var action = _database.Get<OfflineActions>(offlineAction);
                aux = await RestService.DeleteRequest($"/items/images/{action.ElementIdentifier}");
            }

            var response = JsonConvert.DeserializeObject<ResponseWs<object>>(aux);

            if (response.Status.Equals("ok"))
            {
                if (offlineAction == null) return;
                _database.Delete<OfflineActions>(offlineAction);
            }
            else
            {
                if (offlineAction != null) return;
                _database.Insert(new OfflineActions()
                {
                    ElementIdentifier = itemImage.ServerId.ToString(),
                    ElementType = "itemImage",
                    Type = "delete"
                });
            }
        }

        #endregion

        #region OfflineActions

        public IEnumerable<OfflineActions> GetOfflineActions()
        {
            return _database.Table<OfflineActions>().ToList();
        }

        #endregion

        #region User

        private void CreateUser()
        {
            _database.InsertOrReplace(new User
            {
                Id = 1
            });
        }

        public async Task<User> GetUser(int id = 1, bool sync = true)
        {
            if (sync && DateTime.Now.Subtract(LastSyncedUser).TotalMinutes > 5)
            {
                var aux = await RestService.GetRequest("/user");
                var response = JsonConvert.DeserializeObject<ResponseWs<User>>(aux);
                if (response.Status.Equals("ok"))
                {
                    response.Data.Id = 1;
                    _database.UpdateWithChildren(response.Data);
                    LastSyncedUser = DateTime.Now;
                }
                else
                {
                    await Xamarin.Forms.Shell.Current.DisplayAlert(Resources.Culture.Strings.Error,
                        response.Message, Resources.Culture.Strings.Ok);
                }
            }

            return _database.Get<User>(id);
        }

        public async Task UpdateUser(User user, int? offlineAction = null)
        {
            if (offlineAction == null)
            {
                _database.Update(user);
            }
            
            var form = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(user));
            var files = new[] {new FileResult(user.File)};

            var aux = await RestService.PostRequest("user/", form, files);
            var response = JsonConvert.DeserializeObject<ResponseWs<object>>(aux);
            if (response.Status.Equals("ok"))
            {
                if(offlineAction == null) return;
                _database.Delete<OfflineActions>(offlineAction);
            }
            else
            {
                if (offlineAction != null) return;

                _database.Insert(new OfflineActions()
                {
                    Type = "update",
                    ElementIdentifier = user.Id.ToString(),
                    ElementType = "user"
                });
                
                await Xamarin.Forms.Shell.Current.DisplayAlert(Resources.Culture.Strings.Error, response.Message,
                    Resources.Culture.Strings.Ok);
            }
        }

        public Apikey GetApikey(string token)
        {
            return _database.Get<Apikey>(token);
        }

        public async Task<IEnumerable<Apikey>> GetApiKeys()
        {
            if (DateTime.Now.Subtract(LastSyncedApiKeys).TotalMinutes > 5)
            {
                var aux = await RestService.GetRequest("/user/api-keys");
                var response = JsonConvert.DeserializeObject<ResponseWs<IEnumerable<Apikey>>>(aux);
                if (response.Status.Equals("ok"))
                {
                    _database.DeleteAll<Apikey>();
                    _database.InsertAll(response.Data);
                    LastSyncedApiKeys = DateTime.Now;
                }
                else
                {
                    await Xamarin.Forms.Shell.Current.DisplayAlert(Resources.Culture.Strings.Error,
                        response.Message, Resources.Culture.Strings.Ok);
                }
            }

            return _database.Table<Apikey>().ToList();
        }

        public async Task UpdateApikey(Apikey apikey, int? offlineAction = null)
        {
            if (offlineAction == null)
            {
                _database.Update(apikey);
            }

            var form = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(apikey));

            var aux = await RestService.PostRequest("user/api-keys/", form);
            var response = JsonConvert.DeserializeObject<ResponseWs<object>>(aux);
            if (response.Status.Equals("ok"))
            {
                if (offlineAction == null) return;
                _database.Delete<OfflineActions>(offlineAction);
            }
            else
            {
                if (offlineAction != null) return;
                
                _database.Insert(new OfflineActions()
                {
                    Type = "update",
                    ElementIdentifier = apikey.Token,
                    ElementType = "apikey"
                });

                await Xamarin.Forms.Shell.Current.DisplayAlert(Resources.Culture.Strings.Error,
                    response.Message, Resources.Culture.Strings.Ok);
            }
        }

        public async Task RemoveApikey(string apikey, int? offlineAction = null)
        {
            if (offlineAction == null)
            {
                _database.Delete<Apikey>(apikey);
            }

            var aux = await RestService.DeleteRequest($"user/api-keys/{apikey}");
            var response = JsonConvert.DeserializeObject<ResponseWs<object>>(aux);
            if (response.Status.Equals("ok"))
            {
                if (offlineAction == null) return;
                _database.Delete<OfflineActions>(offlineAction);
            }
            else
            {
                if (offlineAction != null) return;

                _database.Insert(new OfflineActions()
                {
                    ElementIdentifier = apikey,
                    ElementType = "apikey",
                    Type = "delete"
                });

                await Xamarin.Forms.Shell.Current.DisplayAlert(Resources.Culture.Strings.Error,
                    response.Message, Resources.Culture.Strings.Ok);
            }
        }

        #endregion
    }
}