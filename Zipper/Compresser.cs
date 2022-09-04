using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Zipper
{
    static public class Compresser
    {
        private static byte[] _sig = new byte[] { 0xFA, 0xAC };

        public static FileInfo InitFile(in string resFilePath)
        {
            if (resFilePath is null)
                throw new ArgumentNullException(nameof(resFilePath));

            var fileInfo = new FileInfo(resFilePath);
            using var sw = new BinaryWriter(fileInfo.OpenWrite());

            sw.Write(_sig);
            return fileInfo;
        }

        public static void Encode(in string resFilePath, in IEnumerable<string> filesPaths, in IEnumerable<string> foldersPaths)
        {
            FileInfo fInfo = InitFile(resFilePath);
            using var sw = new BinaryWriter(fInfo.OpenWrite());
            sw.Seek(2, SeekOrigin.Begin);//Skip signature.
            long filesDataStartPosition = 2L;

            var foldersStoreInfo = GetFoldersPathsToStore(foldersPaths);
            sw.Write(0L);
            filesDataStartPosition += 8L;
            Dictionary<string, long> foldersPointers = new(foldersStoreInfo.Count);
            sw.Write(foldersStoreInfo.Count);
            filesDataStartPosition += 4L;
            foreach (var folder in foldersStoreInfo)
            {
                foldersPointers.Add(folder, sw.BaseStream.Position);
                var folderInfoBytes = Encoding.UTF8.GetBytes(folder);
                sw.Write(folderInfoBytes.Length);
                sw.Write(folderInfoBytes);
                filesDataStartPosition += 4 + folderInfoBytes.Length;
            }
            int tmpPtr = (int)sw.BaseStream.Position;
            sw.Seek(2, SeekOrigin.Begin);
            sw.Write(filesDataStartPosition);
            sw.Seek(tmpPtr, SeekOrigin.Begin);

            var allFiles = GetAllFilesFromDirectories(foldersPaths);
            allFiles.AddRange(filesPaths);

            sw.Write(allFiles.Count);
            foreach (var f in allFiles.Select(p => new FileInfo(p)))
            {
                var dInfo = foldersPointers.Keys.FirstOrDefault(k => f.DirectoryName?.EndsWith(k) is true);
                if (dInfo is not null)
                    sw.Write(foldersPointers[dInfo]);
                else
                    sw.Write(0L);

                byte[] nameBytes = Encoding.UTF8.GetBytes(f.Name);

                sw.Write(nameBytes.Length);
                sw.Write(nameBytes);
                sw.Write(f.Length);
                sw.Write(File.ReadAllBytes(f.FullName));
            }
        }

        private static List<string> GetAllFilesFromDirectories(in IEnumerable<string> foldersPaths)
        {
            List<string> ans = new();
            foreach (var folder in foldersPaths)
            {
                var files = Directory.EnumerateFiles(folder);
                if (files.Any())
                    foreach (var file in files)
                        ans.Add(file);

                var subFolders = Directory.EnumerateDirectories(folder);
                if (subFolders.Any())
                    ans.AddRange(GetAllFilesFromDirectories(subFolders));
            }
            return ans;
        }

        private static List<string> GetFoldersPathsToStore(in IEnumerable<string> foldersPaths)
        {
            List<string> ans = new(foldersPaths.Count());
            foreach (var dir in foldersPaths)
                ans.AddRange(ProcessFolderPath(dir));
            return ans;
        }

        private static List<string> ProcessFolderPath(in string folderPath, in string prevBuildedPath = "")
        {
            List<string> ans = new();

            var dinfo = new DirectoryInfo(folderPath);
            var files = Directory.EnumerateFiles(folderPath);
            var dirs = Directory.EnumerateDirectories(folderPath);
            if (!files.Any() && !dirs.Any())
            {
                ans.Add(Path.Combine(prevBuildedPath, dinfo.Name));
                return ans;
            }
            if (dirs.Any())
            {
                foreach (var folder in dirs)
                    ans.AddRange(ProcessFolderPath(folder, Path.Combine(prevBuildedPath, dinfo.Name)));
            }
            if (files.Any())
            {
                ans.Add(Path.Combine(prevBuildedPath, dinfo.Name));
            }

            return ans;
        }

        public static void Decode(in string filePath, in string folderPath)
        {
            if (filePath is null)
                throw new ArgumentNullException(nameof(filePath));
            if (folderPath is null)
                throw new ArgumentNullException(nameof(folderPath));

            using var sr = new BinaryReader(File.OpenRead(filePath));

            byte[] fSig = sr.ReadBytes(2);
            if (!_sig.SequenceEqual(fSig))
                throw new ArgumentException("Signature invalid");

            long toSkipBytes = sr.ReadInt64();
            int dirsCont = sr.ReadInt32();
            for (int i = 0; i < dirsCont; i++)
            {
                int len = sr.ReadInt32();
                string folderShortPath = Encoding.UTF8.GetString(sr.ReadBytes(len));
                string folderPathToCreate = Path.Combine(folderPath, folderShortPath);
                Directory.CreateDirectory(folderPathToCreate);
            }

            sr.BaseStream.Position = toSkipBytes;

            int filesCount = sr.ReadInt32();
            for (int i = 0; i < filesCount; i++)
            {
                string folderInfo = string.Empty;
                long folderInfoPtr = sr.ReadInt64();
                if(folderInfoPtr != 0L)
                    folderInfo = GetStringFromPointer(sr, folderInfoPtr);


                int nameLen = sr.ReadInt32();
                string fileName = Encoding.UTF8.GetString(sr.ReadBytes(nameLen));
                long fileLen = sr.ReadInt64();

                string fileFullPath = Path.Combine(folderPath, folderInfo, fileName);
                File.WriteAllBytes(fileFullPath, sr.ReadBytes((int)fileLen));//if >2gb file will err.
            }
        }

        private static string GetStringFromPointer(in BinaryReader br, long pointer)
        {
            long currentPtr = br.BaseStream.Position;
            br.BaseStream.Position = pointer;

            var ansLen = br.ReadInt32();
            string ans = Encoding.UTF8.GetString(br.ReadBytes(ansLen));

            br.BaseStream.Position = currentPtr;
            return ans;
        }

    }
}
