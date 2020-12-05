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
#if DEBUG
        private const string BaseUrl = "http://192.168.14.23:3000";
#else
        private const string BaseUrl = "HOSTURL";
#endif
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
                _connection.CreateTable<Category>();
                _connection.CreateTable<Subcategory>();
                _connection.CreateTable<Collection>();
                _connection.CreateTable<Item>();
                _connection.CreateTable<ItemImage>();

                CreateCategories();
                CreateSubcategories();
            }
            catch (Exception ex)
            {
                AppCenterUtils.ReportException(ex, "Database Creation");
            }
        }

        #region Categories and Subcategories Creation

        private void CreateCategories()
        {
            var categories = new List<Category>()
            {
                new Category()
                {
                    Spanish = "Antigüedades",
                    English = "Antiques",
                    Catalan = "",
                    Basque = "",
                    Id = 1
                },
                new Category()
                {
                    Spanish = "Arqueología e historia natural",
                    English = "Archaeology & Natural Story",
                    Catalan = "",
                    Basque = "",
                    Id = 2
                },
                new Category()
                {
                    Spanish = "Arte",
                    English = "Art",
                    Catalan = "",
                    Basque = "",
                    Id = 3
                },
                new Category()
                {
                    Spanish = "Artesanía",
                    English = "Craftwork",
                    Catalan = "",
                    Basque = "",
                    Id = 4
                },
                new Category()
                {
                    Spanish = "Bebidas y envases",
                    English = "Drinks & Bottles",
                    Catalan = "",
                    Basque = "",
                    Id = 5
                },
                new Category()
                {
                    Spanish = "Cámaras Fotográficas",
                    English = "Cameras",
                    Catalan = "",
                    Basque = "",
                    Id = 6
                },
                new Category()
                {
                    Spanish = "Cine y Series",
                    English = "Cinema & TV Shows",
                    Catalan = "",
                    Basque = "",
                    Id = 7
                },
                new Category()
                {
                    Spanish = "Medios de transporte",
                    English = "",
                    Catalan = "",
                    Basque = "",
                    Id = 8
                },
                new Category()
                {
                    Spanish = "Comunicación y sonido",
                    English = "",
                    Catalan = "",
                    Basque = "",
                    Id = 9
                },
                new Category()
                {
                    Spanish = "Costura y manualidades",
                    English = "Sewing & Handicraft",
                    Catalan = "",
                    Basque = "",
                    Id = 10
                },
                new Category()
                {
                    Spanish = "Deportes y eventos",
                    English = "Sports & Events",
                    Catalan = "",
                    Basque = "",
                    Id = 11
                },
                new Category()
                {
                    Spanish = "Informática y electrónica",
                    English = "",
                    Catalan = "",
                    Basque = "",
                    Id = 12
                },
                new Category()
                {
                    Spanish = "Interiores y decoración",
                    English = "",
                    Catalan = "",
                    Basque = "",
                    Id = 13
                },
                new Category()
                {
                    Spanish = "Joyería",
                    English = "Fashion",
                    Catalan = "",
                    Basque = "",
                    Id = 14
                },
                new Category()
                {
                    Spanish = "Juguetes y Juegos",
                    English = "Toys & Games",
                    Catalan = "",
                    Basque = "",
                    Id = 15
                },
                new Category()
                {
                    Spanish = "Libros, Cómics y Manga",
                    English = "Books, comics & Manga",
                    Catalan = "",
                    Basque = "",
                    Id = 16
                },
                new Category()
                {
                    Spanish = "Militaría y Armas",
                    English = "Military & Weaponry",
                    Catalan = "",
                    Basque = "",
                    Id = 17
                },
                new Category()
                {
                    Spanish = "Monedas y Billetes",
                    English = "Coins & ",
                    Catalan = "",
                    Basque = "",
                    Id = 18
                },
                new Category()
                {
                    Spanish = "Música",
                    English = "Music",
                    Catalan = "",
                    Basque = "",
                    Id = 19
                },
                new Category()
                {
                    Spanish = "Otros",
                    English = "Other",
                    Catalan = "",
                    Basque = "",
                    Id = 20
                },
                new Category()
                {
                    Spanish = "Papelería",
                    English = "",
                    Catalan = "",
                    Basque = "",
                    Id = 21
                },
                new Category()
                {
                    Spanish = "Relojes",
                    English = "",
                    Catalan = "",
                    Basque = "",
                    Id = 22
                },
                new Category()
                {
                    Spanish = "Sellos",
                    English = "Stamps",
                    Catalan = "",
                    Basque = "",
                    Id = 23
                },
                new Category()
                {
                    Spanish = "Textil",
                    English = "",
                    Catalan = "",
                    Basque = "",
                    Id = 24
                },
            };

            foreach (var group in categories)
            {
                _connection.InsertOrReplace(group);
            }
        }

        private void CreateSubcategories()
        {
            var subcategories = new List<Subcategory>()
            {
                new Subcategory()
                {
                    Id = 1,
                    CategoryId = 1,
                    Spanish = "Muebles antiguos",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 2,
                    CategoryId = 1,
                    Spanish = "Porcelana y cerámica",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 3,
                    CategoryId = 1,
                    Spanish = "Otros",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 4,
                    CategoryId = 2,
                    Spanish = "Fósiles",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 5,
                    CategoryId = 2,
                    Spanish = "Minerales y rocas",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 6,
                    CategoryId = 5,
                    Spanish = "Tapones",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 7,
                    CategoryId = 5,
                    Spanish = "Cervezas",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 8,
                    CategoryId = 5,
                    Spanish = "Vinos",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 9,
                    CategoryId = 5,
                    Spanish = "Placas de cava",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 10,
                    CategoryId = 5,
                    Spanish = "Cavas",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 11,
                    CategoryId = 7,
                    Spanish = "DVD",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 12,
                    CategoryId = 7,
                    Spanish = "VHS",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 13,
                    CategoryId = 7,
                    Spanish = "Blu-ray",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 14,
                    CategoryId = 7,
                    Spanish = "Material autografiado",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 15,
                    CategoryId = 8,
                    Spanish = "Coches",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 16,
                    CategoryId = 8,
                    Spanish = "Motocicletas",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 17,
                    CategoryId = 8,
                    Spanish = "Bicicletas",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 18,
                    CategoryId = 9,
                    Spanish = "Hi-Fi y radio",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 19,
                    CategoryId = 9,
                    Spanish = "Gramófonos",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 20,
                    CategoryId = 9,
                    Spanish = "Tocadiscos",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 21,
                    CategoryId = 9,
                    Spanish = "Teléfonos",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 22,
                    CategoryId = 9,
                    Spanish = "Televisores",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 23,
                    CategoryId = 10,
                    Spanish = "Dedales",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 24,
                    CategoryId = 11,
                    Spanish = "Material deportivo",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 25,
                    CategoryId = 11,
                    Spanish = "Ropa y complementos",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 26,
                    CategoryId = 11,
                    Spanish = "Material autografiado",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 27,
                    CategoryId = 12,
                    Spanish = "Ordenadores y accesorios",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 28,
                    CategoryId = 12,
                    Spanish = "Monitores",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 29,
                    CategoryId = 12,
                    Spanish = "Impresoras y accesorios",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 30,
                    CategoryId = 12,
                    Spanish = "Software",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 31,
                    CategoryId = 13,
                    Spanish = "Figuras",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 32,
                    CategoryId = 13,
                    Spanish = "Imanes",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 33,
                    CategoryId = 15,
                    Spanish = "Figuras",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 34,
                    CategoryId = 15,
                    Spanish = "Funkos",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 35,
                    CategoryId = 15,
                    Spanish = "Maquetas",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 36,
                    CategoryId = 15,
                    Spanish = "Material autografiado",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 37,
                    CategoryId = 16,
                    Spanish = "Cartografía",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 38,
                    CategoryId = 16,
                    Spanish = "Libros",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 39,
                    CategoryId = 16,
                    Spanish = "Cómics",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 40,
                    CategoryId = 16,
                    Spanish = "Libros - Arte y fotografía",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 41,
                    CategoryId = 16,
                    Spanish = "Material autografiado",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 42,
                    CategoryId = 18,
                    Spanish = "Monedas periodo antiguo",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 43,
                    CategoryId = 18,
                    Spanish = "Monedas periodo moderno",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 44,
                    CategoryId = 18,
                    Spanish = "Monedas España",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 45,
                    CategoryId = 18,
                    Spanish = "Monedas extranjeras",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 46,
                    CategoryId = 18,
                    Spanish = "Notafilia - Billetes",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 47,
                    CategoryId = 19,
                    Spanish = "Discos",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 48,
                    CategoryId = 19,
                    Spanish = "Vinilos",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 49,
                    CategoryId = 19,
                    Spanish = "Instrumentos musicales",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 50,
                    CategoryId = 19,
                    Spanish = "Material autografiado",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 51,
                    CategoryId = 21,
                    Spanish = "Cromos y álbumes",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 52,
                    CategoryId = 21,
                    Spanish = "Pegatinas",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 53,
                    CategoryId = 21,
                    Spanish = "Material autografiado",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 54,
                    CategoryId = 21,
                    Spanish = "Fotografías",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 55,
                    CategoryId = 21,
                    Spanish = "Postales",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 56,
                    CategoryId = 22,
                    Spanish = "Relojes de pulsera",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 57,
                    CategoryId = 22,
                    Spanish = "Relojes de pared",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
                new Subcategory()
                {
                    Id = 58,
                    CategoryId = 22,
                    Spanish = "Relojes de bolsillo",
                    English = "",
                    Catalan = "",
                    Basque = ""
                },
            };

            foreach (var subcategory in subcategories)
            {
                _connection.InsertOrReplace(subcategory);
            }
        }

        #endregion

        #region Backup and Delete

        public bool CreateBackup()
        {
            _connection.Close();

            var ret = FileSystemUtils.BackupDataAndDatabase(_databasePath);

            _connection = new SQLiteConnection(_databasePath);

            return ret;
        }

        public bool RestoreBackup()
        {
            _connection.Close();

            var ret = FileSystemUtils.RestoreBackupDataAndDatabase(_databasePath);

            _connection = new SQLiteConnection(_databasePath);

            return ret;
        }

        public bool DeleteAllData()
        {
            _connection.Close();

            var ret = FileSystemUtils.DeleteAllData(_databasePath);

            _connection = new SQLiteConnection(_databasePath);

            return ret;
        }

        #endregion

        #region Categories & GetCollectionGroups

        public IEnumerable<Category> GetCategories()
        {
            return _connection.Table<Category>().ToList();
        }

        public IEnumerable<CollectionGroup> GetCollectionGroups()
        {
            var ret = new ObservableCollection<CollectionGroup>();

            var groups = GetCategories();

            foreach (var collectionGroup in from category in groups
                let collections = new ObservableCollection<Collection>(GetCollectionsByGroupId(category.Id.ToString()))
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

        public void AddCategories(ref IEnumerable<Category> categories)
        {
            _connection.Insert(categories);
        }

        public void UpdateCategory(Category category)
        {
            _connection.Update(category);
        }

        public void DeleteCategory(string id)
        {
            _connection.Delete<Category>(id);
        }

        #endregion

        #region Subcategories

        public IEnumerable<Subcategory> GetSubcategoriesByCategoryId(string categoryId)
        {
            return _connection.Query<Subcategory>("Select * from Subcategory Where CategoryId = ?", categoryId);
        }

        public Subcategory GetSubcategory(string id)
        {
            return _connection.Get<Subcategory>(id);
        }

        public void AddSubcategories(ref IEnumerable<Subcategory> subcategories)
        {
            _connection.Insert(subcategories);
        }

        public void UpdateSubcategory(Subcategory subcategory)
        {
            _connection.Update(subcategory);
        }

        public void DeleteSubcategory(string id)
        {
            _connection.Delete<Subcategory>(id);
        }

        #endregion

        #region Collection

        public Collection GetCollection(string id, bool withChildren = false)
        {
            if (withChildren)
            {
                return _connection.GetWithChildren<Collection>(id);
            }
            else
            {
                return _connection.Get<Collection>(id);
            }
        }

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

        public void RemoveCollection(string id)
        {
            _connection.Query<Item>("Delete from Item Where CollectionId = ?", id);
            _connection.Delete<Collection>(id);
        }

        public IEnumerable<Collection> GetCollectionsByGroupId(string id)
        {
            return _connection.Query<Collection>("Select * from Collection Where CategoryId = ?", id);
        }

        #endregion

        #region Item

        public Item GetItem(string id, bool withChildren = false)
        {
            if (withChildren)
            {
                return _connection.GetWithChildren<Item>(id);
            }
            else
            {
                return _connection.Get<Item>(id);
            }
        }

        public IEnumerable<Item> GetAllItemsFromCategory(string collectionId, bool withChildren = false)
        {
            if (withChildren)
            {
                var collId = int.Parse(collectionId);
                return _connection.GetAllWithChildren<Item>(item => item.CollectionId == collId);
            }
            else
            {
                return _connection.Query<Item>("Select * From Item WHERE CollectionId = ?", collectionId);
            }
        }

        public void AddItem(ref Item item, bool recursive = false)
        {
            if (recursive)
            {
                _connection.InsertWithChildren(item, true);
            }
            else
            {
                _connection.Insert(item);
            }
        }

        public void UpdateItem(Item item, bool recursive = false)
        {
            if (recursive)
            {
                _connection.UpdateWithChildren(item);
            }
            else
            {
                _connection.Update(item);
            }
        }

        public void RemoveItem(string id)
        {
            _connection.Query<ItemImage>("Delete from ItemImage Where ItemId = ?", id);
            _connection.Delete<Item>(id);
        }

        #endregion

        #region ItemImages

        public ItemImage GetItemImage(string id)
        {
            return _connection.Get<ItemImage>(id);
        }

        public void AddItemImage(ItemImage itemImage)
        {
            _connection.Insert(itemImage);
        }

        public void UpdateItemImage(ItemImage itemImage)
        {
            _connection.Update(itemImage);
        }

        public void RemoveItemImage(string id)
        {
            _connection.Delete<ItemImage>(id);
        }

        #endregion
    }
}