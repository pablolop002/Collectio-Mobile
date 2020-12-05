using System;
using System.Collections.Generic;
using System.Linq;
using Collectio.Resources.Culture;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Collectio.Models
{
    public class Item
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }

        [Unique] public int? ServerId { get; set; }

        [ForeignKey(typeof(Collection)), Indexed] public int CollectionId { get; set; }

        [ForeignKey(typeof(Subcategory))] public int SubcategoryId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool Private { get; set; }

        [Ignore] public string PrivateText => Private ? Strings.Private : Strings.Public;

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

        public override bool Equals(object obj)
        {
            return obj is Item other && Equals(other);
        }

        private bool Equals(Item other)
        {
            return Id == other.Id && CollectionId == other.CollectionId && SubcategoryId == other.SubcategoryId &&
                   Name == other.Name && Description == other.Description && Private == other.Private &&
                   Equals(Images, other.Images);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ ServerId.GetHashCode();
                hashCode = (hashCode * 397) ^ CollectionId;
                hashCode = (hashCode * 397) ^ SubcategoryId;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Private.GetHashCode();
                hashCode = (hashCode * 397) ^ CreatedAt.GetHashCode();
                hashCode = (hashCode * 397) ^ (Images != null ? Images.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}