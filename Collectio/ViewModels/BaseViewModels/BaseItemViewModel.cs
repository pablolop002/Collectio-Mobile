using Collectio.Models;
using MvvmHelpers;

namespace Collectio.ViewModels.BaseViewModels
{
    public class BaseItemViewModel : BaseViewModel
    {
        protected Item _item;

        private bool _isbnVisible;
        private bool _authorVisible;
        private bool _colorVisible;
        private bool _sizeVisible;
        private bool _mediumVisible;
        private bool _conditionVisible;
        private bool _scientificNameVisible;

        public Item Item
        {
            get => _item;
            set => SetProperty(ref _item, value);
        }

        public bool Isbn
        {
            get => _isbnVisible;
            set
            {
                if (!SetProperty(ref _isbnVisible, value)) return;
                if (value) return;
                _item.Isbn = string.Empty;
            }
        }

        public bool Author
        {
            get => _authorVisible;
            set
            {
                if (!SetProperty(ref _authorVisible, value)) return;
                if (value) return;
                _item.Author = string.Empty;
            }
        }

        public bool Size
        {
            get => _sizeVisible;
            set
            {
                if (!SetProperty(ref _sizeVisible, value)) return;
                if (value) return;
                _item.Size = string.Empty;
            }
        }

        public bool Color
        {
            get => _colorVisible;
            set
            {
                if (!SetProperty(ref _colorVisible, value)) return;
                if (value) return;
                _item.Color = string.Empty;
            }
        }

        public bool Medium
        {
            get => _mediumVisible;
            set
            {
                if (!SetProperty(ref _mediumVisible, value)) return;
                if (value) return;
                _item.Medium = string.Empty;
            }
        }

        public bool Condition
        {
            get => _conditionVisible;
            set
            {
                if (!SetProperty(ref _conditionVisible, value)) return;
                if (value) return;
                _item.Condition = string.Empty;
            }
        }

        public bool ScientificName
        {
            get => _scientificNameVisible;
            set
            {
                if (!SetProperty(ref _scientificNameVisible, value)) return;
                if (value) return;
                _item.ScientificName = string.Empty;
            }
        }

        public BaseItemViewModel()
        {
            Item = new Item();
        }

        protected bool EnableField(string field)
        {
            switch (field)
            {
                case nameof(Isbn):
                    Isbn = true;
                    return true;
                case nameof(Author):
                    Author = true;
                    return true;
                case nameof(Color):
                    Color = true;
                    return true;
                case nameof(Size):
                    Size = true;
                    return true;
                case nameof(Medium):
                    Medium = true;
                    return true;
                case nameof(Condition):
                    Condition = true;
                    return true;
                case nameof(ScientificName):
                    ScientificName = true;
                    return true;
                case "All":
                    SetAllFields(true);
                    return true;
                default:
                    return false;
            }
        }

        protected void SetAllFields(bool status)
        {
            Isbn = Author = Color = Size = Medium = Condition = status;
        }
    }
}