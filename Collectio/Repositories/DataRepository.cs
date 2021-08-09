using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Collectio.Models;
using Collectio.Resources.Culture;
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

                if (!Preferences.Get("LoggedIn", false)) CreateUser();
                Triggers();
#pragma warning disable 4014
                CreateOrUpdateCategories();
#pragma warning restore 4014
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

        public async Task<bool> CreateOrUpdateCategories()
        {
            var ret = false;
            var aux = await RestService.GetRequest("/categories");
            var response = JsonConvert.DeserializeObject<ResponseWs<IEnumerable<Category>>>(aux);

            if (response == null || response.Status.Equals("ko"))
            {
                if (!_database.Table<Category>().Any())
                {
                    var categories = new List<Category>
                    {
                        new Category
                        {
                            Spanish = "Antigüedades",
                            English = "Antiques",
                            Catalan = "",
                            Basque = "",
                            Id = 1
                        },
                        new Category
                        {
                            Spanish = "Arqueología e historia natural",
                            English = "Archaeology & Natural Story",
                            Catalan = "",
                            Basque = "",
                            Id = 2
                        },
                        new Category
                        {
                            Spanish = "Arte",
                            English = "Art",
                            Catalan = "",
                            Basque = "",
                            Id = 3
                        },
                        new Category
                        {
                            Spanish = "Artesanía",
                            English = "Craftwork",
                            Catalan = "",
                            Basque = "",
                            Id = 4
                        },
                        new Category
                        {
                            Spanish = "Bebidas y envases",
                            English = "Drinks & Bottles",
                            Catalan = "",
                            Basque = "",
                            Id = 5
                        },
                        new Category
                        {
                            Spanish = "Cámaras Fotográficas",
                            English = "Cameras",
                            Catalan = "",
                            Basque = "",
                            Id = 6
                        },
                        new Category
                        {
                            Spanish = "Cine y Series",
                            English = "Cinema & TV Shows",
                            Catalan = "",
                            Basque = "",
                            Id = 7
                        },
                        new Category
                        {
                            Spanish = "Medios de transporte",
                            English = "",
                            Catalan = "",
                            Basque = "",
                            Id = 8
                        },
                        new Category
                        {
                            Spanish = "Comunicación y sonido",
                            English = "",
                            Catalan = "",
                            Basque = "",
                            Id = 9
                        },
                        new Category
                        {
                            Spanish = "Costura y manualidades",
                            English = "Sewing & Handicraft",
                            Catalan = "",
                            Basque = "",
                            Id = 10
                        },
                        new Category
                        {
                            Spanish = "Deportes y eventos",
                            English = "Sports & Events",
                            Catalan = "",
                            Basque = "",
                            Id = 11
                        },
                        new Category
                        {
                            Spanish = "Informática y electrónica",
                            English = "",
                            Catalan = "",
                            Basque = "",
                            Id = 12
                        },
                        new Category
                        {
                            Spanish = "Interiores y decoración",
                            English = "",
                            Catalan = "",
                            Basque = "",
                            Id = 13
                        },
                        new Category
                        {
                            Spanish = "Joyería",
                            English = "Fashion",
                            Catalan = "",
                            Basque = "",
                            Id = 14
                        },
                        new Category
                        {
                            Spanish = "Juguetes y Juegos",
                            English = "Toys & Games",
                            Catalan = "",
                            Basque = "",
                            Id = 15
                        },
                        new Category
                        {
                            Spanish = "Libros, Cómics y Manga",
                            English = "Books, comics & Manga",
                            Catalan = "",
                            Basque = "",
                            Id = 16
                        },
                        new Category
                        {
                            Spanish = "Militaría y Armas",
                            English = "Military & Weaponry",
                            Catalan = "",
                            Basque = "",
                            Id = 17
                        },
                        new Category
                        {
                            Spanish = "Monedas y Billetes",
                            English = "Coins & ",
                            Catalan = "",
                            Basque = "",
                            Id = 18
                        },
                        new Category
                        {
                            Spanish = "Música",
                            English = "Music",
                            Catalan = "",
                            Basque = "",
                            Id = 19
                        },
                        new Category
                        {
                            Spanish = "Otros",
                            English = "Other",
                            Catalan = "",
                            Basque = "",
                            Id = 20
                        },
                        new Category
                        {
                            Spanish = "Papelería",
                            English = "",
                            Catalan = "",
                            Basque = "",
                            Id = 21
                        },
                        new Category
                        {
                            Spanish = "Relojes",
                            English = "",
                            Catalan = "",
                            Basque = "",
                            Id = 22
                        },
                        new Category
                        {
                            Spanish = "Sellos",
                            English = "Stamps",
                            Catalan = "",
                            Basque = "",
                            Id = 23
                        },
                        new Category
                        {
                            Spanish = "Textil",
                            English = "",
                            Catalan = "",
                            Basque = "",
                            Id = 24
                        },
                    };

                    foreach (var category in categories)
                    {
                        _database.Insert(category);
                    }

                    ret = categories.Any();
                }
            }
            else
            {
                foreach (var category in response.Data)
                {
                    _database.InsertOrReplace(category);
                }

                ret = response.Data.Any();
            }

            ret = ret && await CreateOrUpdateSubcategories();
            return ret;
        }

        public IEnumerable<Category> GetCategories()
        {
            return _database.Table<Category>().ToList();
        }

        public IEnumerable<CollectionGroup> GetCollectionGroups()
        {
            var ret = new ObservableRangeCollection<CollectionGroup>();

            if (LoggedIn)
            {
                GetCollectionsFromServer();
            }

            var categories = GetCategories();

            foreach (var collectionGroup in from category in categories
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

        private async Task<bool> CreateOrUpdateSubcategories()
        {
            var ret = false;
            var aux = await RestService.GetRequest("/subcategories");
            var response = JsonConvert.DeserializeObject<ResponseWs<IEnumerable<Subcategory>>>(aux);

            if (response == null || response.Status.Equals("ko"))
            {
                if (!_database.Table<Subcategory>().Any())
                {
                    var subcategories = new List<Subcategory>
                    {
                        new Subcategory
                        {
                            Id = 1,
                            CategoryId = 1,
                            Spanish = "Muebles antiguos",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 2,
                            CategoryId = 1,
                            Spanish = "Porcelana y cerámica",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 3,
                            CategoryId = 1,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 4,
                            CategoryId = 2,
                            Spanish = "Fósiles",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 5,
                            CategoryId = 2,
                            Spanish = "Minerales y rocas",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 6,
                            CategoryId = 5,
                            Spanish = "Tapones",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 7,
                            CategoryId = 5,
                            Spanish = "Cervezas",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 8,
                            CategoryId = 5,
                            Spanish = "Vinos",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 9,
                            CategoryId = 5,
                            Spanish = "Placas de cava",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 10,
                            CategoryId = 5,
                            Spanish = "Cavas",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 11,
                            CategoryId = 7,
                            Spanish = "DVD",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 12,
                            CategoryId = 7,
                            Spanish = "VHS",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 13,
                            CategoryId = 7,
                            Spanish = "Blu-ray",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 14,
                            CategoryId = 7,
                            Spanish = "Material autografiado",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 15,
                            CategoryId = 8,
                            Spanish = "Coches",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 16,
                            CategoryId = 8,
                            Spanish = "Motocicletas",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 17,
                            CategoryId = 8,
                            Spanish = "Bicicletas",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 18,
                            CategoryId = 9,
                            Spanish = "Hi-Fi y radio",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 19,
                            CategoryId = 9,
                            Spanish = "Gramófonos",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 20,
                            CategoryId = 9,
                            Spanish = "Tocadiscos",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 21,
                            CategoryId = 9,
                            Spanish = "Teléfonos",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 22,
                            CategoryId = 9,
                            Spanish = "Televisores",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 23,
                            CategoryId = 10,
                            Spanish = "Dedales",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 24,
                            CategoryId = 11,
                            Spanish = "Material deportivo",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 25,
                            CategoryId = 11,
                            Spanish = "Ropa y complementos",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 26,
                            CategoryId = 11,
                            Spanish = "Material autografiado",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 27,
                            CategoryId = 12,
                            Spanish = "Ordenadores y accesorios",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 28,
                            CategoryId = 12,
                            Spanish = "Monitores",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 29,
                            CategoryId = 12,
                            Spanish = "Impresoras y accesorios",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 30,
                            CategoryId = 12,
                            Spanish = "Software",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 31,
                            CategoryId = 13,
                            Spanish = "Figuras",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 32,
                            CategoryId = 13,
                            Spanish = "Imanes",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 33,
                            CategoryId = 15,
                            Spanish = "Figuras",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 34,
                            CategoryId = 15,
                            Spanish = "Funkos",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 35,
                            CategoryId = 15,
                            Spanish = "Maquetas",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 36,
                            CategoryId = 15,
                            Spanish = "Material autografiado",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 37,
                            CategoryId = 16,
                            Spanish = "Cartografía",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 38,
                            CategoryId = 16,
                            Spanish = "Libros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 39,
                            CategoryId = 16,
                            Spanish = "Cómics",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 40,
                            CategoryId = 16,
                            Spanish = "Libros - Arte y fotografía",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 41,
                            CategoryId = 16,
                            Spanish = "Material autografiado",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 42,
                            CategoryId = 18,
                            Spanish = "Monedas periodo antiguo",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 43,
                            CategoryId = 18,
                            Spanish = "Monedas periodo moderno",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 44,
                            CategoryId = 18,
                            Spanish = "Monedas España",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 45,
                            CategoryId = 18,
                            Spanish = "Monedas extranjeras",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 46,
                            CategoryId = 18,
                            Spanish = "Notafilia - Billetes",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 47,
                            CategoryId = 19,
                            Spanish = "Discos",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 48,
                            CategoryId = 19,
                            Spanish = "Vinilos",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 49,
                            CategoryId = 19,
                            Spanish = "Instrumentos musicales",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 50,
                            CategoryId = 19,
                            Spanish = "Material autografiado",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 51,
                            CategoryId = 21,
                            Spanish = "Cromos y álbumes",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 52,
                            CategoryId = 21,
                            Spanish = "Pegatinas",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 53,
                            CategoryId = 21,
                            Spanish = "Material autografiado",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 54,
                            CategoryId = 21,
                            Spanish = "Fotografías",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 55,
                            CategoryId = 21,
                            Spanish = "Postales",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 56,
                            CategoryId = 22,
                            Spanish = "Relojes de pulsera",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 57,
                            CategoryId = 22,
                            Spanish = "Relojes de pared",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 58,
                            CategoryId = 22,
                            Spanish = "Relojes de bolsillo",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 59,
                            CategoryId = 2,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 60,
                            CategoryId = 3,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 61,
                            CategoryId = 4,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 62,
                            CategoryId = 5,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 63,
                            CategoryId = 6,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 64,
                            CategoryId = 7,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 65,
                            CategoryId = 8,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 66,
                            CategoryId = 9,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 67,
                            CategoryId = 10,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 68,
                            CategoryId = 11,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 69,
                            CategoryId = 12,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 70,
                            CategoryId = 13,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 71,
                            CategoryId = 14,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 72,
                            CategoryId = 15,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 73,
                            CategoryId = 16,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 74,
                            CategoryId = 17,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 75,
                            CategoryId = 18,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 76,
                            CategoryId = 19,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 77,
                            CategoryId = 20,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 78,
                            CategoryId = 21,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 79,
                            CategoryId = 22,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 80,
                            CategoryId = 23,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                        new Subcategory
                        {
                            Id = 81,
                            CategoryId = 24,
                            Spanish = "Otros",
                            English = "",
                            Catalan = "",
                            Basque = ""
                        },
                    };

                    foreach (var subcategory in subcategories)
                    {
                        _database.Insert(subcategory);
                    }

                    ret = subcategories.Any();
                }
            }
            else
            {
                foreach (var subcategory in response.Data)
                {
                    _database.InsertOrReplace(subcategory);
                }

                ret = response.Data.Any();
            }

            return ret;
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

        public async Task<bool> UploadAllData()
        {
            var collections = _database.GetAllWithChildren<Collection>(collection => true, true);
            if (collections == null || collections.Count == 0) return true;

            foreach (var collection in collections)
            {
                var items = collection.Items;
                collection.Items = null;
                var form = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    JsonConvert.SerializeObject(collection));

                var aux = await RestService.PutRequest("/collections", form,
                    string.IsNullOrWhiteSpace(collection.File) ? null : new[] { new FileResult(collection.File) });
                try
                {
                    var response = JsonConvert.DeserializeObject<ResponseWs<int?>>(aux);

                    if (response is { Status: "ok", Data: { } })
                    {
                        collection.ServerId = response.Data;
                        _database.Update(collection);

                        foreach (var item in items)
                        {
                            item.CollectionServerId = collection.ServerId;
                            var itemImages = item.Images;
                            item.Images = null;
                            var itemForm =
                                JsonConvert.DeserializeObject<Dictionary<string, string>>(
                                    JsonConvert.SerializeObject(item));
                            if (itemForm != null && itemForm.ContainsKey("CollectionServerId"))
                            {
                                if (string.IsNullOrWhiteSpace(itemForm["CollectionServerId"]) ||
                                    !itemForm["CollectionServerId"].Equals(collection.ServerId.ToString()))
                                {
                                    itemForm["CollectionServerId"] = collection.ServerId.ToString();
                                    item.CollectionServerId = collection.ServerId;
                                }
                            }
                            else
                            {
                                itemForm?.Add("CollectionServerId", collection.ServerId.ToString());
                                item.CollectionServerId = collection.ServerId;
                            }

                            var itemAux = await RestService.PutRequest("/items",
                                itemForm /*, item.Images.Select(itemImage => new FileResult(itemImage.File)).ToArray()*/);

                            try
                            {
                                var itemResponse =
                                    JsonConvert.DeserializeObject<ResponseWs<Dictionary<string, int>>>(itemAux);
                                if (itemResponse is { Status: "ok", Data: { } })
                                {
                                    item.ServerId = itemResponse.Data["item"];
                                    _database.Update(item);

                                    foreach (var itemImage in itemImages)
                                    {
                                        itemImage.ItemServerId = item.ServerId;

                                        var itemImageForm =
                                            JsonConvert.DeserializeObject<Dictionary<string, string>>(
                                                JsonConvert.SerializeObject(itemImage));
                                        var itemImageAux = await RestService.PutRequest("/items/images", itemImageForm,
                                            new[] { new FileResult(itemImage.File) });
                                        try
                                        {
                                            var itemImageResponse =
                                                JsonConvert
                                                    .DeserializeObject<ResponseWs<Dictionary<string, int>>>(
                                                        itemImageAux);
                                            if (itemImageResponse is { Status: "ok", Data : { } })
                                            {
                                                itemImage.ServerId = itemImageResponse.Data[itemImage.Image];
                                                _database.Update(itemImage);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            AppCenterUtils.ReportException(ex, "UploadAllDataItemImage");
                                        }
                                    }
                                    /*foreach (var ids in itemResponse.Data)
                                    {
                                        if (ids.Key.Equals("item")) continue;
        
                                        var itemImage = item.Images.Find(e => e.Image.Equals(ids.Key));
                                        itemImage.ServerId = ids.Value;
                                        _database.Update(itemImage);
                                    }*/
                                }
                                else
                                {
                                    //await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, $"Upload Error. Item: {item.Name}", Strings.Ok);
                                }
                            }
                            catch (Exception ex)
                            {
                                AppCenterUtils.ReportException(ex, "UploadAllDataItem");
                            }
                        }
                    }
                    else
                    {
                        //await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, $"Upload Error Collection: {collection.Name}", Strings.Ok);
                    }
                }
                catch (Exception ex)
                {
                    AppCenterUtils.ReportException(ex, "UploadAllDataCollection");
                }
            }

            return true;
        }

        #region Collection

        private async void GetCollectionsFromServer()
        {
            if (DateTime.Now.Subtract(LastSyncedCollections).TotalMinutes > 5)
            {
                var request = await RestService.GetRequest("/collections?withChildren=true");
                var responseWs = JsonConvert.DeserializeObject<ResponseWs<List<Collection>>>(request);
                if (responseWs == null) return;
                if (responseWs.Status.Equals("ok"))
                {
                    _database.DropTable<ItemImage>();
                    _database.DropTable<Item>();
                    _database.DropTable<Collection>();
                    _database.CreateTable<Collection>();
                    _database.CreateTable<Item>();
                    _database.CreateTable<ItemImage>();
                    FileSystemUtils.DeleteAllData();

                    foreach (var collection in responseWs.Data)
                    {
                        collection.Items ??= new List<Item>();

                        _database.Insert(collection);
                        if (collection.Image != null)
                        {
                            await FileSystemUtils.SaveFileFromServer(collection.Image, GetUserServerId(),
                                collection.ServerId);
                        }

                        foreach (var item in collection.Items)
                        {
                            item.CollectionId = collection.Id;
                            _database.Insert(item);

                            if (item.Images == null || !item.Images.Any())
                            {
                                continue;
                            }

                            foreach (var itemImage in item.Images)
                            {
                                _database.Insert(itemImage);
                                await FileSystemUtils.SaveFileFromServer(itemImage.Image, GetUserServerId(),
                                    collection.ServerId, item.ServerId);
                            }
                        }
                    }

                    LastSyncedCollections = DateTime.Now;
                }
            }
        }

        public Collection GetCollection(string id, bool withChildren = false)
        {
            return withChildren ? _database.GetWithChildren<Collection>(id) : _database.Get<Collection>(id);
        }

        public async Task<Collection> GetServerCollection(string id, bool withChildren = false)
        {
            var aux = await RestService.GetRequest(
                $"/collections?collection={id}&withChildren={withChildren.ToString()}");
            var response = JsonConvert.DeserializeObject<ResponseWs<Collection>>(aux);
            if (response != null && response.Status.Equals("ok", StringComparison.CurrentCultureIgnoreCase))
            {
                return response.Data;
            }

            return null;
        }

        public IEnumerable<Collection> GetAllCollections()
        {
            return _database.GetAllWithChildren<Collection>(c => true);
        }

        public async Task<int> AddCollection(Collection collection, bool fromServer = false)
        {
            if (LoggedIn && !fromServer)
            {
                var form = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    JsonConvert.SerializeObject(collection));
                var files = collection.Image == null ? null : new[] { new FileResult(collection.TempFile) };
                var aux = await RestService.PutRequest("/collections", form, files);
                var response = JsonConvert.DeserializeObject<ResponseWs<int?>>(aux);
                if (response is { Status: "ok" })
                {
                    collection.ServerId = response.Data;
                    _database.Insert(collection);
                    return collection.Id;
                }
            }
            else
            {
                _database.Insert(collection);
                return collection.Id;
            }

            return -1;
        }

        public async Task<bool> UpdateCollection(Collection collection)
        {
            if (LoggedIn && collection.ServerId != null)
            {
                var oldCollection = _database.Get<Collection>(collection.Id.ToString());

                var form = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    JsonConvert.SerializeObject(collection));

                var files = collection.Image == oldCollection.Image
                    ? null
                    : new[] { new FileResult(collection.File) };

                var aux = await RestService.PostRequest("/collections", form, files);
                var response = JsonConvert.DeserializeObject<ResponseWs<int?>>(aux);

                if (response is { Status: "ko" })
                {
                    return false;
                }
            }

            _database.Update(collection);
            return true;
        }

        public async Task<bool> RemoveCollection(string id)
        {
            var collection = GetCollection(id, true);

            if (LoggedIn && collection?.ServerId != null)
            {
                var aux = await RestService.DeleteRequest($"/collections/{collection.ServerId.ToString()}");

                var response = JsonConvert.DeserializeObject<ResponseWs<object>>(aux);

                if (response is { Status: "ko" })
                {
                    return false;
                }
            }

            if (collection == null) return true;

            foreach (var item in collection.Items)
            {
                foreach (var itemImage in item.Images)
                {
                    _database.Delete(itemImage);
                }

                _database.Delete(item);
            }

            _database.Delete(collection);

            return true;
        }

        private IEnumerable<Collection> GetCollectionsByGroupId(string id)
        {
            return _database.Query<Collection>("Select * from Collection Where CategoryId = ?", id);
        }

        #endregion

        #region Item

        public Item GetItem(string id, bool withChildren = false)
        {
            return withChildren ? _database.GetWithChildren<Item>(id) : _database.Get<Item>(id);
        }

        public IEnumerable<Item> GetAllItemsFromCollection(string collectionId, bool withChildren = false)
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

        public async Task<int> AddItem(Item item)
        {
            if (LoggedIn)
            {
                var collection = _database.Get<Collection>(item.CollectionId.ToString());
                if (collection.ServerId == null)
                {
                    return -1;
                }

                var form = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(item));
                if (form != null && form.ContainsKey("CollectionServerId"))
                {
                    if (string.IsNullOrWhiteSpace(form["CollectionServerId"]) || !form["CollectionServerId"].Equals(collection.ServerId.ToString()))
                    {
                        form["CollectionServerId"] = collection.ServerId.ToString();
                        item.CollectionServerId = collection.ServerId;
                    }
                }
                else
                {
                    form?.Add("CollectionServerId", collection.ServerId.ToString());
                    item.CollectionServerId = collection.ServerId;
                }

                var images = item.Images?.Select(image => new FileResult(image.TempFile)).ToArray();

                var aux = await RestService.PutRequest("/items", form, images);
                var response = JsonConvert.DeserializeObject<ResponseWs<Dictionary<string, int>>>(aux);

                if (response is { Status: "ok" })
                {
                    item.ServerId = response.Data["item"];
                    _database.Insert(item);

                    if (item.Images != null)
                    {
                        foreach (var elem in response.Data.Where(elem => !elem.Key.Equals("item")))
                        {
                            var itemImage = item.Images.Find(e => e.Image.Equals(elem.Key));
                            itemImage.ServerId = elem.Value;
                            itemImage.ItemId = item.Id;

                            _database.Insert(itemImage);
                        }
                    }

                    return item.Id;
                }
            }
            else
            {
                _database.Insert(item);

                if (item.Images != null)
                {
                    foreach (var image in item.Images)
                    {
                        image.ItemId = item.Id;
                        _database.Insert(image);
                    }
                }

                return item.Id;
            }

            return -1;
        }

        public async Task<bool> UpdateItem(Item item, List<ItemImage> newImages = null,
            List<ItemImage> imagesToDelete = null)
        {
            if (LoggedIn)
            {
                var collection = _database.Get<Collection>(item.CollectionId.ToString());

                if (collection.ServerId == null)
                {
                    return false;
                }

                var form = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                    JsonConvert.SerializeObject(item));

                if (form == null)
                {
                    AppCenterUtils.ReportException(new Exception("FormCreationException"), "UpdateItem");
                    return false;
                }

                if (form.ContainsKey("CollectionServerId"))
                {
                    if (string.IsNullOrWhiteSpace(form["CollectionServerId"]) || !form["CollectionServerId"].Equals(collection.ServerId.ToString()))
                    {
                        form["CollectionServerId"] = collection.ServerId.ToString();
                        item.CollectionServerId = collection.ServerId;
                    }
                }
                else
                {
                    form.Add("CollectionServerId", collection.ServerId.ToString());
                    item.CollectionServerId = collection.ServerId;
                }

                var files = new List<FileResult>();

                if (newImages != null)
                {
                    files.AddRange(newImages.Select(image => new FileResult(image.TempFile)));
                }

                if (item.ServerId != null)
                {
                    if (imagesToDelete != null)
                    {
                        form.Add("ImagesToDelete",
                            JsonConvert.SerializeObject((from image in imagesToDelete
                                where image.ServerId != null
                                select image.ServerId.ToString()).ToList()));
                    }

                    var aux = await RestService.PostRequest("/items", form, files.ToArray());
                    var response = JsonConvert.DeserializeObject<ResponseWs<Dictionary<string, int>>>(aux);

                    if (response is { Status: "ok" })
                    {
                        _database.Update(item);

                        if (newImages != null)
                        {
                            foreach (var image in newImages.Where(image => response.Data.ContainsKey(image.Image)))
                            {
                                image.ServerId = response.Data[image.Image];
                                _database.Insert(image);
                            }
                        }

                        if (imagesToDelete != null)
                        {
                            foreach (var image in imagesToDelete)
                            {
                                _database.Delete<ItemImage>(image.Id.ToString());
                            }
                        }

                        return true;
                    }
                }
                else
                {
                    /*foreach (var image in item.Images)
                    {
                        if (imagesToDelete == null || !imagesToDelete.Contains(image))
                        {
                            files.Add(new FileResult(image.File));
                        }
                    }*/

                    var aux = await RestService.PutRequest("/items", form, files.ToArray());
                    var response = JsonConvert.DeserializeObject<ResponseWs<Dictionary<string, int>>>(aux);

                    if (response is { Status: "ok" })
                    {
                        item.ServerId = response.Data["item"];
                        _database.Update(item);

                        if (imagesToDelete != null)
                        {
                            foreach (var image in imagesToDelete)
                            {
                                _database.Delete<ItemImage>(image.Id.ToString());
                            }
                        }

                        if (item.Images != null)
                        {
                            foreach (var itemImage in item.Images)
                            {
                                var itemImageForm =
                                    JsonConvert.DeserializeObject<Dictionary<string, string>>(
                                        JsonConvert.SerializeObject(itemImage));
                                if (itemImageForm != null && itemImageForm.ContainsKey("ItemServerId"))
                                {
                                    if (string.IsNullOrWhiteSpace(itemImageForm["ItemServerId"]) ||!itemImageForm["ItemServerId"].Equals(item.ServerId.ToString()))
                                    {
                                        itemImageForm["ItemServerId"] = item.ServerId.ToString();
                                        itemImage.ItemServerId = item.ServerId;
                                    }
                                }
                                else
                                {
                                    itemImageForm?.Add("ItemServerId", item.ServerId.ToString());
                                    itemImage.ItemServerId = item.ServerId;
                                }

                                var itemImageFile = new[] { new FileResult(itemImage.File) };

                                var itemImageAux =
                                    await RestService.PutRequest("/items/images", itemImageForm, itemImageFile);

                                var itemImageResponse =
                                    JsonConvert.DeserializeObject<ResponseWs<Dictionary<string, int>>>(itemImageAux);

                                if (itemImageResponse is { Status: "ok", Data: { } })
                                {
                                    if (itemImageResponse.Data.ContainsKey(itemImage.Image))
                                    {
                                        itemImage.ServerId = itemImageResponse.Data[itemImage.Image];
                                    }
                                }
                            }
                        }

                        /*foreach (var elem in response.Data.Where(elem => !elem.Key.Equals("item")))
                        {
                            var itemImage = item.Images.Find(e => e.Image.Equals(elem.Key));
                            itemImage.ServerId = elem.Value;
                            itemImage.ItemId = item.Id;

                            _database.InsertOrReplace(itemImage);
                        }*/

                        return true;
                    }
                }
            }
            else
            {
                if (newImages != null)
                {
                    foreach (var image in newImages)
                    {
                        _database.Insert(image);
                    }
                }

                if (imagesToDelete != null)
                {
                    foreach (var image in imagesToDelete)
                    {
                        _database.Delete<ItemImage>(image.Id.ToString());
                    }
                }

                _database.Update(item);

                return true;
            }

            return false;
        }

        public async Task<bool> RemoveItem(string id)
        {
            var item = GetItem(id);

            if (LoggedIn && item.ServerId != null)
            {
                var aux = await RestService.DeleteRequest($"/items/{item.ServerId.ToString()}");
                var response = JsonConvert.DeserializeObject<ResponseWs<object>>(aux);

                if (response is { Status: "ko" })
                {
                    return false;
                }
            }

            _database.Delete(item);
            return true;
        }

        #endregion

        #region ItemImages

        public ItemImage GetItemImage(string id)
        {
            return _database.Get<ItemImage>(id);
        }

        public async Task<bool> AddItemImage(ItemImage itemImage)
        {
            var item = _database.Get<Item>(itemImage.ItemId.ToString());

            if (LoggedIn && itemImage.ServerId != null)
            {
                var form = new Dictionary<string, string>
                {
                    { "Image", itemImage.Image },
                    { "ItemServerId", item.ServerId.ToString() }
                };

                var aux =
                    await RestService.PutRequest("/items/images", form, new[] { new FileResult(itemImage.File) });
                var response = JsonConvert.DeserializeObject<ResponseWs<Dictionary<string, int>>>(aux);

                if (response is { Status: "ok", Data: { } })
                {
                    if (response.Data.ContainsKey(itemImage.Image))
                    {
                        itemImage.ServerId = response.Data[itemImage.Image];
                    }

                    return true;
                }
            }
            else
            {
                _database.Insert(itemImage);
                return true;
            }

            return false;
        }

        public async Task<bool> RemoveItemImage(string id)
        {
            var itemImage = GetItemImage(id);

            if (LoggedIn && itemImage.ServerId != null)
            {
                var aux = await RestService.DeleteRequest($"/items/images/{itemImage.ServerId.ToString()}");

                var response = JsonConvert.DeserializeObject<ResponseWs<object>>(aux);

                if (response is { Status: "ok" })
                {
                    _database.Delete(itemImage);
                    return true;
                }
            }
            else
            {
                _database.Delete(itemImage);
                return true;
            }

            return false;
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

                if (response != null)
                {
                    if (response.Status.Equals("ok"))
                    {
                        response.Data.Id = 1;
                        _database.Update(response.Data);
                        LastSyncedUser = DateTime.Now;
                    }
                    else
                    {
                        try
                        {
                            await Xamarin.Forms.Shell.Current.DisplayAlert(Strings.Error, response.Message, Strings.Ok);
                        }
                        catch (Exception ex)
                        {
                            AppCenterUtils.ReportException(ex, "GetUser");
                        }
                    }
                }
            }

            return _database.Get<User>(id.ToString());
        }

        public int? GetUserServerId()
        {
            return _database.Get<User>("1").ServerId;
        }

        public async Task<bool> UpdateUser(User user)
        {
            var form = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(user));

            var files = string.IsNullOrWhiteSpace(user.File) ? null : new[] { new FileResult(user.File) };

            var aux = await RestService.PostRequest("/user", form, files);
            var response = JsonConvert.DeserializeObject<ResponseWs<object>>(aux);

            if (response is { Status: "ok" })
            {
                _database.Update(user);
                return true;
            }

            return false;
        }

        public async Task<bool> RemoveUserImage(User user)
        {
            var aux = await RestService.DeleteRequest("/user/image");
            var response = JsonConvert.DeserializeObject<ResponseWs<object>>(aux);

            if (response is { Status: "ok" })
            {
                _database.Update(user);
                return true;
            }

            return false;
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

                if (response != null)
                {
                    if (response.Status.Equals("ok"))
                    {
                        _database.DeleteAll<Apikey>();
                        _database.InsertAll(response.Data);
                        LastSyncedApiKeys = DateTime.Now;
                    }
                }
            }

            return _database.Table<Apikey>().ToList();
        }

        public async Task<bool> UpdateApikey(Apikey apikey)
        {
            var form = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(apikey));

            var aux = await RestService.PostRequest("/user/api-keys", form);
            var response = JsonConvert.DeserializeObject<ResponseWs<object>>(aux);

            if (response is { Status: "ok" })
            {
                _database.Update(apikey);
                return true;
            }

            return false;
        }

        public async Task<bool> RemoveApikey(string apikey)
        {
            var aux = await RestService.DeleteRequest($"/user/api-keys/{apikey}");
            var response = JsonConvert.DeserializeObject<ResponseWs<object>>(aux);

            if (response is { Status: "ok" })
            {
                _database.Delete<Apikey>(apikey);
                return true;
            }

            return false;
        }

        #endregion
    }
}