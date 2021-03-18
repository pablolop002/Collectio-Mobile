using SQLite;

namespace Collectio.Models
{
    public class OfflineActions
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        
        public string Type { get; set; }
        
        public string ElementType { get; set; }
        
        public string ElementIdentifier { get; set; }
        
        public bool ImageUpdated { get; set; }
    }
}