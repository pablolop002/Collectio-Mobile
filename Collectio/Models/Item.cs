using System.Collections.Generic;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Collectio.Models
{
    public class Item
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        [ForeignKey(typeof(Collection)), Indexed] public int CollectionId { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)] public IEnumerable<ItemImage> Images { get; set; }
    }
}