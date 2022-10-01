using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Zipper.Commands;

namespace Zipper.ViewModels
{
    public class CompresserViewModel : ViewModelBase
    {
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

        public string DefaultEncodedFolderPath
        {
            get => _defaultEncodedFolderPath ?? "Не задано";
            set => Set(ref _defaultEncodedFolderPath, value);
        }

        #endregion NotyProps

        private ICommand? _decodeCommand = null;

        public ICommand DecodeCommand
        {
            get
            {
                if (_decodeCommand is not null)
                    return _decodeCommand;

                _decodeCommand = new LambdaCommand(
                    p =>
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
                            _faacFilePath = ofd.FileName;
                        }

                        if (faacSelectRes is true)
                        {
                            if (string.IsNullOrWhiteSpace(_defaultEncodedFolderPath))
                            {
                                using var fbd = new System.Windows.Forms.FolderBrowserDialog();
                                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                    _defaultEncodedFolderPath = fbd.SelectedPath;
                                else
                                    _defaultEncodedFolderPath = null;
                            }

                            if (string.IsNullOrEmpty(_defaultEncodedFolderPath))
                            {
                                MessageBox.Show("Folder not selected", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            try
                            {
                                Compresser.Decode(_faacFilePath, _defaultEncodedFolderPath);
                                MessageBox.Show("Decoded", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Decoding error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    },
                    p => true);
                return _decodeCommand;
            }
        }

        private ICommand? _previewDropCommand;
        public ICommand PreviewDropCommand
            => _previewDropCommand ??= new LambdaCommand(HandlePreviewDrop, p => true);

        private void HandlePreviewDrop(object inObject)
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

            bool? dialogRes = true;
            if (string.IsNullOrEmpty(_faacFilePath))
            {
                SaveFileDialog saveDialog = new();
                dialogRes = saveDialog.ShowDialog();
                _faacFilePath = saveDialog.FileName;
            }

            if (dialogRes is true)
                Compresser.Encode(_faacFilePath, filesPaths, foldersPaths);
        }

    }
}
