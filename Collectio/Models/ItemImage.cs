using Collectio.Utils;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Collectio.Models
{
    public class ItemImage
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }

        [ForeignKey(typeof(Item)), Indexed] public int ItemId { get; set; }
        
        public string Image { get; set; }
        
        [Ignore] public string File => FileSystemUtils.GetItemImage(Image, ItemId);
    }
}