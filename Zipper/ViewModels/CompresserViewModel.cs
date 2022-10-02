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
            }
        }

        private static string? GetName(string? path)
        {
            string? name = path?.Substring(path.LastIndexOf(Path.DirectorySeparatorChar));
            return string.IsNullOrWhiteSpace(name) ? null : name;
        }
        public string? FaacFileName => GetName(DevSettings.DefaultFaacFilePath);

        public string? DefaultFilesFolderName => GetName(DevSettings.DefaultFolderToEncodePath);

        public string? DefaultEncodedFolderName => GetName(DevSettings.DefaultFolderToDecodePath);

        private string? _faacFilePath = null;

        //public string? FaacFilePath
        //{
        //    get => _faacFilePath;
        //    set
        //    {
        //        Set(ref _faacFilePath, value);
        //        OnPropertyChaged(nameof(FaacFileName));
        //    }
        //}

        //private string? _defaultFilesFolderPath = null;

        //public string? DefaultFilesFolderPath
        //{
        //    get => _defaultFilesFolderPath;
        //    set
        //    {
        //        Set(ref _defaultFilesFolderPath, value);
        //        OnPropertyChaged(nameof(DefaultEncodedFolderName));
        //    }
        //}

        //private string? _defaultEncodedFolderPath = null;

        //public string? DefaultEncodedFolderPath
        //{
        //    get => _defaultEncodedFolderPath;
        //    set
        //    {
        //        Set(ref _defaultEncodedFolderPath, value);
        //        OnPropertyChaged(nameof(DefaultEncodedFolderName));
        //    }
        //}

        private bool _isAlgoApplying = false;

        public bool IsAlgoApplying
        {
            get => _isAlgoApplying;
            set => Set(ref _isAlgoApplying, value);
        }
        #endregion NotyProps

        #region Commands
        private ICommand? _decodeCommand;

        public ICommand DecodeCommand
            => _decodeCommand ??= new LambdaCommand(StartDecoding);

        private async void StartDecoding(object obj)
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
                    IsAlgoApplying = false;
                    MessageBox.Show($"{faacFilePath}\ndecoded in\n{folderToDecodePath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Decoding error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private ICommand? _previewDropCommand;
        public ICommand PreviewDropCommand
            => _previewDropCommand ??= new LambdaCommand(HandlePreviewDrop);

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
                IsAlgoApplying = true;
                await Task.Run(() =>
                    Compresser.Encode(faacFilePath, filesPaths, foldersPaths)
                );

                IsAlgoApplying = false;
                MessageBox.Show($"Encoded in {faacFilePath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
