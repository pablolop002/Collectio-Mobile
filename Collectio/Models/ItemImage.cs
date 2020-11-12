using Collectio.Utils;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Collectio.Models
{
    public class ItemImage
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        
        public string FileName { get; set; }
        
        [Ignore] public string File => FileSystemUtils.GetItemImage(FileName, ItemId);

        [ForeignKey(typeof(Item)), Indexed] public int ItemId { get; set; }
    }
}