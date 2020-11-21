using Collectio.Utils;
using SQLite;

namespace Collectio.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }

        public string Nickname { get; set; }
        
        public string Mail { get; set; }

        public string Image { get; set; }

        [Ignore] public string File => FileSystemUtils.GetProfileImage(Image);
    }
}