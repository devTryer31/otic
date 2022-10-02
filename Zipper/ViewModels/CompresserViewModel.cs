using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Zipper.Commands;

namespace Zipper.ViewModels
{
    public class CompresserViewModel : ViewModelBase
    {
        //private Task? _currentProcess;
        //private readonly CancellationToken _cancellationToken;

        #region NotyProps

        private string? _faacFileName = null;

        public string FaacFileName
        {
            get => _faacFileName ?? "Не задано";
            set => Set(ref _faacFileName, value);
        }

        private string? _defaultFilesFolder = null;

        public string DefaultFilesFolder
        {
            get => _defaultFilesFolder ?? "Не задано";
            set => Set(ref _defaultFilesFolder, value);
        }

        private string? _defaultEncodedFolder = null;

        public string DefaultEncodedFolder
        {
            get => _defaultEncodedFolder ?? "Не задано";
            set => Set(ref _defaultEncodedFolder, value);
        }

        private string? _faacFilePath = null;

        public string FaacFilePath
        {
            get => _faacFilePath ?? "Не задано";
            set => Set(ref _faacFilePath, value);
        }

        private string? _defaultFilesFolderPath = null;

        public string DefaultFilesFolderPath
        {
            get => _defaultFilesFolderPath ?? "Не задано";
            set => Set(ref _defaultFilesFolderPath, value);
        }

        private string? _defaultEncodedFolderPath = null;

        public string? DefaultEncodedFolderPath
        {
            get => _defaultEncodedFolderPath ?? "Не задано";
            set => Set(ref _defaultEncodedFolderPath, value);
        }

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
            bool? faacSelectRes = true;
            if (string.IsNullOrWhiteSpace(_faacFilePath))
            {
                OpenFileDialog ofd = new()
                {
                    Filter = "faac files(*.faac)|*.faac",
                    Multiselect = false
                };
                faacSelectRes = ofd.ShowDialog();
                FaacFileName = ofd.FileName;
            }

            if (faacSelectRes is true)
            {
                bool folderSelectionRes = false;
                if (string.IsNullOrWhiteSpace(_defaultEncodedFolderPath))
                {
                    using var fbd = new System.Windows.Forms.FolderBrowserDialog();
                    if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        DefaultEncodedFolderPath = fbd.SelectedPath;
                        folderSelectionRes = true;
                    }
                    else
                    {
                        DefaultEncodedFolderPath = null;
                        folderSelectionRes = false;
                    }
                }

                if (string.IsNullOrEmpty(_defaultEncodedFolderPath) && folderSelectionRes)
                {
                    MessageBox.Show("Folder not selected", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                try
                {
                    IsAlgoApplying = true;
                    await Task.Run(() =>
                        Compresser.Decode(_faacFilePath!, _defaultEncodedFolderPath!)
                    );
                    IsAlgoApplying = false;
                    MessageBox.Show($"{_faacFilePath}\ndecoded in\n{_defaultEncodedFolderPath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

            if (string.IsNullOrEmpty(_faacFilePath))
            {
                SaveFileDialog saveDialog = new();
                if (saveDialog.ShowDialog() is true)
                {
                    FaacFilePath = saveDialog.FileName;
                    if (!_faacFilePath!.EndsWith(".faac"))
                        FaacFilePath += ".faac";
                }
            }

            if (!string.IsNullOrWhiteSpace(_faacFilePath))
            {
                IsAlgoApplying = true;
                await Task.Run(() =>
                    Compresser.Encode(_faacFilePath, filesPaths, foldersPaths)
                );

                IsAlgoApplying = false;
                MessageBox.Show($"Encoded in {_faacFilePath}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
