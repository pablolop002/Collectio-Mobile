using System.Collections.ObjectModel;

namespace Collectio.Models
{
    public class CollectionGroup : ObservableCollection<Collection>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public CollectionGroup(string name, ObservableCollection<Collection> collections) : base(collections)
        {
            Name = name;
        }
    }
}