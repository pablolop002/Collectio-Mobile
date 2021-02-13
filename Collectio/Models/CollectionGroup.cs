using MvvmHelpers;

namespace Collectio.Models
{
    public class CollectionGroup : ObservableRangeCollection<Collection>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public CollectionGroup(string name, ObservableRangeCollection<Collection> collections) : base(collections)
        {
            Name = name;
        }
    }
}