namespace Zipper.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly string[] _encodingTypes = new[]
        {
            "Без кодирования",
            "Шеннона-Фано"
        };

        public string[] EncodingTypes
        {
            get => _encodingTypes;
        }

        private string _selectedEncodingType = "Без кодирования";

        public string SelectedEncodingType
        {
            get => _selectedEncodingType;
            set => Set(ref _selectedEncodingType, value);
        }

    }
}
