using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Zipper.Commands;
using Zipper.Models;

namespace Zipper.ViewModels
{
    public class CompresserViewModel : ViewModelBase
    {
        //private Task? _currentProcess;
        //private readonly CancellationToken _cancellationToken;

        public CompresserViewModel(MainWindowViewModel mainWindowViewModel)
        {
            MainWindowViewModel = mainWindowViewModel;
        }
        public MainWindowViewModel MainWindowViewModel { get; }

        #region NotyProps

        private DevSettings _devSettings = new();

        public DevSettings DevSettings
        {
            get => _devSettings;
            set
            {
                Set(ref _devSettings, value);
                OnPropertyChaged(nameof(FaacFileName));
                OnPropertyChaged(nameof(DefaultFilesFolderName));
                OnPropertyChaged(nameof(DefaultEncodedFolderName));
                OnPropertyChaged(nameof(AllSettingsPreseted));

                //AllSettingsPreseted 
            }
        }

        private static string? GetName(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            var attr = File.GetAttributes(path);
            if (attr.HasFlag(FileAttributes.Directory))
                return new DirectoryInfo(path).Name;

            return new FileInfo(path).Name;
        }
        public string? FaacFileName => GetName(DevSettings.DefaultFaacFilePath);

        public string? DefaultFilesFolderName => GetName(DevSettings.DefaultFolderToEncodePath);

        public string? DefaultEncodedFolderName => GetName(DevSettings.DefaultFolderToDecodePath);

        private bool _isAlgoApplying = false;

        public bool IsAlgoApplying
        {
            get => _isAlgoApplying;
            set => Set(ref _isAlgoApplying, value);
        }

        #endregion NotyProps

        #region Commands
        private ICommand? _codeAndDecodeCommand;

        public ICommand CodeAndDecodeCommand
            => _codeAndDecodeCommand ??= new LambdaCommand(StartCodeAndDecode);

        private async void StartCodeAndDecode(object p)
        {
            var sourceFolder = DevSettings.DefaultFolderToEncodePath;
            if (sourceFolder is null)
                throw new ArgumentException("Не установлена папка для кодирования по умолчанию в режиме разработчика");

            var dirs = Directory.EnumerateDirectories(sourceFolder).ToList();
            var files = Directory.EnumerateFiles(sourceFolder).ToList();
            await Encode(files, dirs);
            Decode(p);
        }

        private ICommand? _decodeCommand;

        public ICommand DecodeCommand
            => _decodeCommand ??= new LambdaCommand(Decode);

        private async void Decode(object obj)
        {
            string? faacFilePath = MainWindowViewModel.IsDevModEnabled ? DevSettings.DefaultFaacFilePath : null;

            bool? faacSelectRes = true;
            if (string.IsNullOrWhiteSpace(faacFilePath))
            {
                OpenFileDialog ofd = new()
                {
                    Filter = "faac files(*.faac)|*.faac",
                    Multiselect = false
                };
                faacSelectRes = ofd.ShowDialog();
                faacFilePath = ofd.FileName;
            }

            if (faacSelectRes is true)
            {
                bool folderSelectionRes = false;
                string? folderToDecodePath = MainWindowViewModel.IsDevModEnabled ? DevSettings.DefaultFolderToDecodePath : null;
                if (string.IsNullOrWhiteSpace(folderToDecodePath))
                {
                    using var fbd = new System.Windows.Forms.FolderBrowserDialog();
                    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        folderToDecodePath = fbd.SelectedPath;
                        folderSelectionRes = true;
                    }
                    else
                    {
                        folderToDecodePath = null;
                        folderSelectionRes = false;
                    }
                }

                if (string.IsNullOrEmpty(folderToDecodePath) && folderSelectionRes)
                {
                    MessageBox.Show("Folder not selected", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                try
                {
                    IsAlgoApplying = true;
                    await Task.Run(() =>
                        Compresser.Decode(faacFilePath, folderToDecodePath!)
                    );
                    MessageBox.Show($"{faacFilePath}\ndecoded in\n{folderToDecodePath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Decoding error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsAlgoApplying = false;
                }
            }
        }

        private ICommand? _previewDropCommand;
        public ICommand PreviewDropCommand
            => _previewDropCommand ??= new LambdaCommand(HandlePreviewDrop);

        public bool AllSettingsPreseted =>
            !(string.IsNullOrWhiteSpace(_devSettings.DefaultFolderToDecodePath) ||
            string.IsNullOrWhiteSpace(_devSettings.DefaultFolderToEncodePath) ||
            string.IsNullOrWhiteSpace(_devSettings.DefaultFaacFilePath))
            && MainWindowViewModel.IsDevModEnabled;

        private async void HandlePreviewDrop(object inObject)
        {
            var droppedPaths = (string[])((IDataObject)inObject).GetData(DataFormats.FileDrop);
            var isDirectory = droppedPaths
                .Select(data => File.GetAttributes(data).HasFlag(FileAttributes.Directory))
                .ToList();

            List<string> filesPaths = new();
            List<string> foldersPaths = new();
            for (int i = 0; i < isDirectory.Count; ++i)
                if (isDirectory[i])
                    foldersPaths.Add(droppedPaths[i]);
                else
                    filesPaths.Add(droppedPaths[i]);

            await Encode(filesPaths, foldersPaths);
        }

        private async Task Encode(List<string> filesPaths, List<string> foldersPaths)
        {
            string? faacFilePath = MainWindowViewModel.IsDevModEnabled ? DevSettings.DefaultFaacFilePath : null;

            if (string.IsNullOrEmpty(faacFilePath))
            {
                SaveFileDialog saveDialog = new();
                if (saveDialog.ShowDialog() is true)
                {
                    faacFilePath = saveDialog.FileName;
                    if (!faacFilePath.EndsWith(".faac"))
                        faacFilePath += ".faac";
                }
            }

            if (!string.IsNullOrWhiteSpace(faacFilePath))
            {
                try
                {
                    IsAlgoApplying = true;
                    await Task.Run(() =>
                        Compresser.Encode(faacFilePath, filesPaths, foldersPaths)
                    );
                    MessageBox.Show($"Encoded in {faacFilePath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Encoding error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsAlgoApplying = false;
                }
            }
        }


        //private ICommand? _stopCurrentProcessCommand;
        //public ICommand StopCurrentProcessCommand
        //    => _stopCurrentProcessCommand ??= new LambdaCommand(StopCurrentProcess, p => true);

        //private void StopCurrentProcess(object obj)
        //{
        //    _cancellationToken.ThrowIfCancellationRequested();
        //}

        #endregion Commands
    }
}
