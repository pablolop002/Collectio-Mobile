using System.Collections.ObjectModel;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Collectio.Models
{
    public class Group
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }

        [Unique] public int? ServerId { get; set; }
        
        public string Name { get; set; }

        public string Image { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)] public ObservableCollection<Collection> Collections { get; set; }
    }
}