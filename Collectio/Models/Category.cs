using System.Collections.ObjectModel;
using Collectio.Utils;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Collectio.Models
{
    public class Category
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        
        public string Image { get; set; }

        #region Translations
        
        public string Spanish { get; set; }
        public string English { get; set; }
        public string Catalan { get; set; }
        public string Basque { get; set; }
        
        #endregion

        [Ignore] public string Name
        {
            get
            {
                return Resources.Culture.Strings.Culture.TwoLetterISOLanguageName switch
                {
                    "en" => English,
                    "ca" => Catalan,
                    "eu" => Basque,
                    _ => Spanish
                };
            }
        }

        [Ignore] public string File => FileSystemUtils.GetGroupImage(Image);
        
        [OneToMany(CascadeOperations = CascadeOperation.All)] public ObservableCollection<Collection> Collections { get; set; }
    }
}