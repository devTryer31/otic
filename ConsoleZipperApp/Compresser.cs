using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            using (var sw = new BinaryWriter(fileInfo.OpenWrite()))
            {
                sw.Write(_sig);
            }

            return fileInfo;
        }

        public static void Encode(in string resFilePath, params string[] paths)
        {
            FileInfo fInfo = InitFile(resFilePath);
            using var sw = new BinaryWriter(fInfo.OpenWrite());
            sw.Write(paths.Length);
            foreach (var f in paths.Select(p => new FileInfo(p)))
            {
                using var br = new BinaryReader(f.OpenRead());
                byte[] nameBytes = Encoding.ASCII.GetBytes(f.Name);

                sw.Write(nameBytes.Length);
                sw.Write(nameBytes);
                sw.Write(f.Length);
                sw.Write(br.ReadBytes((int)f.Length));
            }
        }

        public static void Decode(in string filePath)
        {
            if (filePath is null)
                throw new ArgumentNullException(nameof(filePath));



            using var sr = new BinaryReader(File.OpenRead(filePath));
            for (int i = 0; i < _sig.Length; i++)
            {
                if (_sig[i] != sr.ReadByte())
                    throw new ArgumentException("Signature invalid");
            }

            int filesCount = sr.ReadInt32();
            for (int i = 0; i < filesCount; i++)
            {
                int nameLen = sr.ReadInt32();
                string fileName = Encoding.ASCII.GetString(sr.ReadBytes(nameLen));
                using var br = new BinaryReader(File.OpenRead(fileName));
                int fileLen = sr.ReadInt32();

                using var bw = new BinaryWriter(File.OpenRead(fileName));

                bw.Write(sr.ReadBytes(fileLen));
            }

        }

    }
}
