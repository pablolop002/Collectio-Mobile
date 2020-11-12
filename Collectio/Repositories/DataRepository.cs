using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using Collectio.Models;
using Collectio.Utils;
using SQLite;
using SQLiteNetExtensions.Extensions;

namespace Collectio.Repositories
{
    public class DataRepository
    {
        private const string BaseUrl = "https://192.168.1.25";
        private readonly HttpClient _client;
        private SQLiteConnection _connection;
        private readonly string _databasePath;

        public DataRepository()
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri(BaseUrl)
            };

            try
            {
                _databasePath = System.IO.Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "collectio.db");
                _connection = new SQLiteConnection(_databasePath);
                _connection.CreateTable<Group>();
                _connection.CreateTable<Collection>();
                _connection.CreateTable<Item>();
                _connection.CreateTable<ItemImage>();

                CreateGroups();
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "Database Creation");
            }
        }

        private void CreateGroups()
        {
            var groups = new List<Group>()
            {
                new Group()
                {
                    Name = "Grupo 1",
                    Id = 1
                },
                new Group()
                {
                    Name = "Grupo 2",
                    Id = 2
                },
                new Group()
                {
                    Name = "Grupo 3",
                    Id = 3
                },
                new Group()
                {
                    Name = "Grupo 4",
                    Id = 4
                },
                new Group()
                {
                    Name = "Grupo 5",
                    Id = 5
                },
                new Group()
                {
                    Name = "Grupo 6",
                    Id = 6
                },
            };

            foreach (var group in groups)
            {
                _connection.InsertOrReplace(group);
            }
        }

        public bool CreateBackup()
        {
            _connection.Close();

            var ret = FileSystemUtils.BackupDataAndDatabase(_databasePath);

            _connection = new SQLiteConnection(_databasePath);

            return ret;
        }

        #region Server

        private void GetRequest(string parameters)
        {
        }

        private void PostRequest(string body)
        {
        }

        #endregion

        #region CollectionGroup

        public IEnumerable<Group> GetCollectionGroupTypes()
        {
            return _connection.Table<Group>().ToList();
        }

        public IEnumerable<CollectionGroup> GetCollectionGroups(bool forced = false)
        {
            var ret = new ObservableCollection<CollectionGroup>();
            //if(!forced || Connectivity.NetworkAccess != NetworkAccess.Internet) {
            var groups = GetCollectionGroupTypes();

            foreach (var collectionGroup in from @group in groups
                let collections = new ObservableCollection<Collection>(GetCollectionsByGroupId(@group.Id))
                where collections.Count > 0
                select new CollectionGroup(@group.Name, collections)
                {
                    Id = @group.Id
                })
            {
                ret.Add(collectionGroup);
            }
            //} else {
            //}

            return ret;
        }

        public void AddCollectionGroups(IEnumerable<Group> group)
        {
            _connection.Insert(group);
        }

        public void AddCollectionGroup(CollectionGroup collectionGroup)
        {
            _connection.InsertWithChildren(collectionGroup);
        }

        public void DeleteCollectionGroup(int id)
        {
            _connection.Delete<CollectionGroup>(id);
        }

        public void UpdateCollectionGroup(CollectionGroup collectionGroup, bool recursive = false)
        {
            if (recursive)
            {
                _connection.UpdateWithChildren(collectionGroup);
            }
            else
            {
                _connection.Update(collectionGroup);
            }
        }

        #endregion

        #region Collection

        public void AddCollection(ref Collection collection, bool recursive = false)
        {
            if (recursive)
            {
                _connection.InsertWithChildren(collection, true);
            }
            else
            {
                _connection.Insert(collection);
            }
        }

        public void UpdateCollection(Collection collection, bool recursive = false)
        {
            if (recursive)
            {
                _connection.UpdateWithChildren(collection);
            }
            else
            {
                _connection.Update(collection);
            }
        }

        public void RemoveCollection(int id)
        {
            _connection.Query<Item>("Delete from Item Where CollectionId = ?", id);
            _connection.Delete<Collection>(id);
        }

        public Collection GetCollection(string id)
        {
            return _connection.Get<Collection>(id);
        }

        public IEnumerable<Collection> GetCollectionsByGroupId(int id)
        {
            return _connection.Query<Collection>("Select * from Collection Where GroupId = ?", id.ToString());
        }

        #endregion

        #region Item

        public Item GetItem(string id)
        {
            return _connection.Get<Item>(id);
        }

        public void EditItem(Item item)
        {
            _connection.Update(item);
        }

        public void RemoveItem(string id)
        {
            _connection.Delete<Item>(id);
        }

        public void AddItem(ref Item item)
        {
            _connection.Insert(item);
        }

        #endregion
    }
}