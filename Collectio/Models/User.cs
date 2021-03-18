using Collectio.Utils;
using MvvmHelpers;
using Newtonsoft.Json;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace Collectio.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        
        [Unique] public int? ServerId { get; set; }

        public string Nickname { get; set; }
        
        public string Mail { get; set; }

        public string Image { get; set; }
        
        public bool Apple { get; set; }
        
        public bool Google { get; set; }
        
        public bool Microsoft { get; set; }
        
        [OneToMany(CascadeOperations = CascadeOperation.All)] public ObservableRangeCollection<Apikey> ApiKeys { get; set; }

        [Ignore, JsonIgnore] public string File => FileSystemUtils.GetProfileImage(Image);
    }
}