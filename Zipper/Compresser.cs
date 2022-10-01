﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Zipper
{
    static public class Compresser
    {
        private static byte[] _sig = new byte[] { 0xFA, 0xAA, 0xAA, 0xAC };
        private const int _version = 1;
        private const byte _byteCompressWithContext = 0;
        private const byte _byteCompress = 0;
        private const byte _byteInterferenceProtection = 0;
        private const byte _byteBuffer = 0;

        private static readonly int _headerBytesCount = _sig.Length + 4 + 1 * 4;

        public static FileInfo InitFile(in string resFilePath)
        {
            if (resFilePath is null)
                throw new ArgumentNullException(nameof(resFilePath));

            var fileInfo = new FileInfo(resFilePath);
            using var sw = new BinaryWriter(fileInfo.Open(FileMode.Create));

            sw.Write(_sig);
            sw.Write(_version);
            sw.Write(_byteCompressWithContext);
            sw.Write(_byteCompress);
            sw.Write(_byteInterferenceProtection);
            sw.Write(_byteBuffer);
            return fileInfo;
        }

        public static void Encode(in string resFilePath, in IEnumerable<string> filesPaths, in IEnumerable<string> foldersPaths)
        {
            FileInfo fInfo = InitFile(resFilePath);
            using var sw = new BinaryWriter(fInfo.OpenWrite());
            sw.Seek(_headerBytesCount, SeekOrigin.Begin);//Skip signature.
            long filesDataStartPosition = _headerBytesCount;

            var foldersStoreInfo = GetFoldersPathsToStore(foldersPaths);
            sw.Write(0L); //Write files data start pointer. Write 0L now, later we rewrite it. 
            filesDataStartPosition += 8L;
            //Writing folders data.
            Dictionary<string, long> foldersPointers = new(foldersStoreInfo.Count);
            sw.Write(foldersStoreInfo.Count);
            filesDataStartPosition += 4L;
            foreach (var folder in foldersStoreInfo)
            {
                //Folder item is [lenName][Name] struct. Ex: a\b\as\ => '7a\b\as\' in bytes presentation.
                foldersPointers.Add(folder, sw.BaseStream.Position);
                var folderInfoBytes = Encoding.UTF8.GetBytes(folder);
                sw.Write(folderInfoBytes.Length);
                sw.Write(folderInfoBytes);
                filesDataStartPosition += 4 + folderInfoBytes.Length;
            }
            //Rewrite files data start pointer.
            int tmpPtr = (int)sw.BaseStream.Position;
            sw.Seek(_headerBytesCount, SeekOrigin.Begin);
            sw.Write(filesDataStartPosition);
            sw.Seek(tmpPtr, SeekOrigin.Begin);

            var allFiles = GetAllFilesFromDirectories(foldersPaths);
            allFiles.AddRange(filesPaths);

            //Writing all files (with in directories) data.
            //File item is [folderInfoPointer][lenName][Name][lenData][Data] struct.
            sw.Write(allFiles.Count);
            foreach (var f in allFiles.Select(p => new FileInfo(p)))
            {
                //Write folder info pointer written above.
                var dInfo = foldersPointers.Keys.FirstOrDefault(k => f.DirectoryName?.EndsWith(k) is true);
                if (dInfo is not null)
                    sw.Write(foldersPointers[dInfo]);
                else
                    sw.Write(0L);

                byte[] nameBytes = Encoding.UTF8.GetBytes(f.Name);
                sw.Write(nameBytes.Length);
                sw.Write(nameBytes);

                var fileData = File.ReadAllBytes(f.FullName);//Can broke on large files >2Gb.

                var tree = new SFTree(fileData);
                var freqBytes = tree.GetFrequenciesInBytes();
                sw.Write(freqBytes.Count);
                sw.Write(freqBytes.ToArray());

                var codedFileData = tree.EncodeBytes().ToArray();

                sw.Write(f.Length);
                sw.Write(codedFileData.Length);
                sw.Write(codedFileData);
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

            byte[] fSig = sr.ReadBytes(_sig.Length);
            if (!_sig.SequenceEqual(fSig))
                throw new ArgumentException("Signature invalid");

            int version = sr.ReadInt32();
            if (version != _version)
                throw new ArgumentException($"The version {version} is not supported. Current is {_version}");

            byte byteCompressWithContext = sr.ReadByte();
            if (byteCompressWithContext != _byteCompressWithContext)
                throw new ArgumentException($"The compress with context version {byteCompressWithContext} is not supported. Current is {_byteCompressWithContext}");
            byte byteCompress = sr.ReadByte();
            if (byteCompress != _byteCompress)
                throw new ArgumentException($"The compress without context version {byteCompress} is not supported. Current is {_byteCompress}");
            byte byteInterferenceProtection = sr.ReadByte();
            if (byteInterferenceProtection != _byteInterferenceProtection)
                throw new ArgumentException($"The interference protection version {byteInterferenceProtection} is not supported. Current is {_byteInterferenceProtection}");
            byte byteBuffer = sr.ReadByte();

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
                if (folderInfoPtr != 0L)
                    folderInfo = GetStringFromPointer(sr, folderInfoPtr);


                int nameLen = sr.ReadInt32();
                string fileName = Encoding.UTF8.GetString(sr.ReadBytes(nameLen));

                int freqsLen = sr.ReadInt32();
                var freqsInBytes = sr.ReadBytes(freqsLen);

                long fileLen = sr.ReadInt64();
                int encodedDataLen = sr.ReadInt32();
                var encodedData = sr.ReadBytes(encodedDataLen);

                var decodedData = SFTree.DecodeBytes(encodedData, (int)fileLen, freqsInBytes.ToList());

                string fileFullPath = Path.Combine(folderPath, folderInfo, fileName);
                File.WriteAllBytes(fileFullPath, decodedData.ToArray());//if >2gb file will err.
                System.Diagnostics.Debug.WriteLine(fileName + "ЗАПИСАНО");
            }
        }

        /// <summary>
        /// Read string from file by <paramref name="br"/> in pointer <paramref name="pointer"/>
        /// with struct [stringLen][string] in bytes representation.
        /// </summary>
        /// <returns>Readed string.</returns>
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
