using System;
using System.Collections.Generic;
using System.Linq;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Collectio.Models
{
    public class Item
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }

        [ForeignKey(typeof(Collection)), Indexed] public int CollectionId { get; set; }
        
        [ForeignKey(typeof(Subcategory))] public int SubcategoryId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
        
        public bool Private { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)] public List<ItemImage> Images { get; set; }

        [Ignore] public string File {
            get
            {
                if (Images == null) return "";
                var img = Images.FirstOrDefault();
                return img != null ? img.File : "";
            }
        }
}

}