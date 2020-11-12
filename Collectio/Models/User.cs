using Collectio.Utils;
using SQLite;

namespace Collectio.Models
{
    public class User
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }

        public string Nickname { get; set; }

        public string FileName { get; set; }

        [Ignore] public string File => FileSystemUtils.GetProfileImage(FileName);
    }
}