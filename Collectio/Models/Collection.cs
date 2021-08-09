using System;
using System.Collections.Generic;
using Collectio.Resources.Culture;
using Collectio.Utils;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Collectio.Models
{
    public class Collection
    {
        [PrimaryKey, AutoIncrement, JsonIgnore] public int Id { get; set; }

        [Unique] public int? ServerId { get; set; }

        [ForeignKey(typeof(User)), JsonIgnore] public int? UserId { get; set; } = 1;

        public int? UserServerId { get; set; }

        [Indexed, ForeignKey(typeof(Category))] public int CategoryId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        public bool Private { get; set; }

        [Ignore, JsonIgnore] public string PrivateText => Private ? Strings.Private : Strings.Public;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)] public List<Item> Items { get; set; }

        [Ignore, JsonIgnore] public string File => FileSystemUtils.GetCollectionImage(Image, Id);

        [Ignore, JsonIgnore]
        public string TempFile => System.IO.Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "Temp", Image);

        public override bool Equals(object obj)
        {
            return obj is Collection other && Equals(other);
        }

        private bool Equals(Collection other)
        {
            return Name == other.Name && Description == other.Description && Image == other.Image &&
                   Private == other.Private && CategoryId == other.CategoryId;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Description != null ? Description.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Image != null ? Image.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Private.GetHashCode();
                hashCode = (hashCode * 397) ^ CategoryId;
                return hashCode;
            }
        }
    }
}