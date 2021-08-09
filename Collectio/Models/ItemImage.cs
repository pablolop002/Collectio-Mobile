using Collectio.Utils;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Collectio.Models
{
    public class ItemImage
    {
        [PrimaryKey, AutoIncrement, JsonIgnore] public int Id { get; set; }

        [Unique] public int? ServerId { get; set; }

        [ForeignKey(typeof(Item)), Indexed, JsonIgnore] public int ItemId { get; set; }

        public int? ItemServerId { get; set; }

        public string Image { get; set; }

        [Ignore, JsonIgnore] public string File => FileSystemUtils.GetItemImage(Image, ItemId);

        [Ignore, JsonIgnore]
        public string TempFile => System.IO.Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "Temp", Image);

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