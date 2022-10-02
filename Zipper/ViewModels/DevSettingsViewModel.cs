using System;
using System.IO;
using System.Xml.Serialization;
using Zipper.Models;

namespace Zipper.ViewModels
{
    public class DevSettingsViewModel : ViewModelBase, IDisposable
    {
        private readonly DevSettings _settings;
        private static readonly XmlSerializer _serializer = new(typeof(DevSettings));
        private readonly string _settingsFilePath;

        public DevSettingsViewModel()
        {
            _settingsFilePath = Path.Combine(new FileInfo(Environment.ProcessPath!).Directory!.FullName, DevSettings.FileName);

            if (File.Exists(_settingsFilePath))
            {
                using var sr = new FileStream(_settingsFilePath, FileMode.Open);
                _settings = (_serializer.Deserialize(sr) as DevSettings)!;
            }

            _settings ??= new DevSettings();
        }

        public string? DefaultFolderToEncodePath
        {
            get => Settings.DefaultFolderToEncodePath;
            set => Settings.DefaultFolderToEncodePath = value;
        }

        public string? DefaultFolderToDecodePath
        {
            get => Settings.DefaultFolderToDecodePath;
            set => Settings.DefaultFolderToDecodePath = value;
        }

        public string? DefaultFaacFilePath
        {
            get => Settings.DefaultFaacFilePath;
            set => Settings.DefaultFaacFilePath = value;
        }

        public DevSettings Settings => _settings;

        public void Dispose()
        {
            using var sw = new FileStream(_settingsFilePath, FileMode.OpenOrCreate);
            _serializer.Serialize(sw, Settings);
        }
    }
}
