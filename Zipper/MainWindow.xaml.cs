using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Zipper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string? _folderPathToDecode = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnDecodeBtnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                Filter = "faac files(*.faac)|*.faac",
                Multiselect = false
            };

            if (ofd.ShowDialog() is true)
            {
                if (string.IsNullOrEmpty(_folderPathToDecode))
                {
                    MessageBox.Show("Folder not selected", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                Compresser.Decode(ofd.FileName, _folderPathToDecode);
            }

            MessageBox.Show("Decoded", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnSelectFolderClick(object sender, RoutedEventArgs e)
        {
            using var fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                _folderPathToDecode = fbd.SelectedPath;
            else
                _folderPathToDecode = null;
        }

        private void OnFilesDropped(object sender, DragEventArgs e)
        {
            var droppedPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
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

            SaveFileDialog saveDialog = new SaveFileDialog();
            if (saveDialog.ShowDialog() is true)
                Compresser.Encode(saveDialog.FileName, filesPaths, foldersPaths);
        }
    }
}
