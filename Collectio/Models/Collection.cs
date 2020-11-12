using System.Collections.ObjectModel;
using Collectio.Utils;
using SQLite;
using SQLiteNetExtensions.Attributes;
using Xamarin.Forms;

namespace Collectio.Models
{
    public class Collection
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        
        [Unique] public int? ServerId { get; set; }
        
        public string Name { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }
        
        [Ignore] public string File => FileSystemUtils.GetCollectionImage(Image, Id);

        public bool Private { get; set; }

        [Indexed, ForeignKey(typeof(CollectionGroup))] public int GroupId { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)] public ObservableCollection<Item> Items { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Collection other && Equals(other);
        }

        private bool Equals(Collection other)
        {
            return Name == other.Name && Description == other.Description && Image == other.Image &&
                   Private == other.Private && GroupId == other.GroupId;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Image != null ? Image.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Private.GetHashCode();
                hashCode = (hashCode * 397) ^ GroupId;
                return hashCode;
            }
        }
    }
}