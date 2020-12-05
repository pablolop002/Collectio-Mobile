using Collectio.Utils;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Collectio.Models
{
    public class ItemImage
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        
        [Unique] public int? ServerId { get; set; }

        [ForeignKey(typeof(Item)), Indexed] public int ItemId { get; set; }
        
        public string Image { get; set; }
        
        [Ignore] public string File => FileSystemUtils.GetItemImage(Image, ItemId);
        
        
        public override bool Equals(object obj)
        {
            return obj is ItemImage other && Equals(other);
        }

        private bool Equals(ItemImage other)
        {
            return Id == other.Id && ItemId == other.ItemId && Image == other.Image;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ ItemId;
                hashCode = (hashCode * 397) ^ (Image != null ? Image.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}