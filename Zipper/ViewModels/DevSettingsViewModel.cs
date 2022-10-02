namespace Zipper.ViewModels
{
    public class DevSettingsViewModel : ViewModelBase
    {
        private string? _defaultFolderToEncodePath;

        public string? DefaultFolderToEncodePath
        {
            get => _defaultFolderToEncodePath;
            set => Set(ref _defaultFolderToEncodePath, value);
        }

        private string? _defaultFolderToDecodePath;

        public string? DefaultFolderToDecodePath
        {
            get => _defaultFolderToDecodePath;
            set => Set(ref _defaultFolderToDecodePath, value);
        }

        private string? _defaultFaacFilePath;

        public string? DefaultFaacFilePath
        {
            get => _defaultFaacFilePath;
            set => Set(ref _defaultFaacFilePath, value);
        }
    }
}
