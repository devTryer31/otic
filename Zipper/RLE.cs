using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Zipper
{
    public class RLE : IDisposable
    {
        private readonly Stream _dataStream;

        public RLE(Stream dataStream)
        {
            _dataStream = dataStream;
        }

        public IEnumerable<byte> Encode()
        {
            List<byte> encodedBytes = new();
            var sr = new BinaryReader(_dataStream);

            List<byte> bufferBytes = new();
            byte prev = sr.ReadByte();
            int countSameBytes = 1;
            while (sr.BaseStream.Position != sr.BaseStream.Length)
            {
                byte cur = sr.ReadByte();
                if (cur == prev)
                {
                    countSameBytes++;
                    continue;
                }

                if (countSameBytes >= 3)
                {
                    if (bufferBytes.Any())
                    {
                        encodedBytes.Add((byte)bufferBytes.Count);
                        encodedBytes.AddRange(bufferBytes);
                        bufferBytes.Clear();
                    }
                    while (countSameBytes > 127)
                    {
                        encodedBytes.Add(127 + 128);
                        encodedBytes.Add(prev);
                        countSameBytes -= 127;
                    }
                    encodedBytes.Add((byte)(countSameBytes + 128));
                    encodedBytes.Add(prev);
                    prev = cur;
                    countSameBytes = 1;
                }
                else
                {
                    bufferBytes.AddRange(Enumerable.Repeat(prev, countSameBytes));
                    if (bufferBytes.Count >= 127)
                    {
                        encodedBytes.Add(127);
                        encodedBytes.AddRange(bufferBytes.Take(127));
                        bufferBytes.RemoveRange(0, 127);
                    }
                    prev = cur;
                    countSameBytes = 1;
                }
            }
            if (countSameBytes >= 3)
            {
                while (countSameBytes > 127)
                {
                    encodedBytes.Add(127 + 128);
                    encodedBytes.Add(prev);
                    countSameBytes -= 127;
                }
                if (countSameBytes >= 3)
                {
                    encodedBytes.Add((byte)(countSameBytes + 128));
                    encodedBytes.Add(prev);
                }
                else
                    encodedBytes.AddRange(Enumerable.Repeat(prev, countSameBytes));
            }
            else
            if (bufferBytes.Any())
            {
                if (bufferBytes.Count >= 127)
                {
                    encodedBytes.Add(127);
                    encodedBytes.AddRange(bufferBytes.Take(127));
                    bufferBytes.RemoveRange(0, 127);
                }
                if (bufferBytes.Count >= 127)
                    throw new Exception(message: "Error buffer RLE encode"); // For sure, never called

                encodedBytes.Add((byte)(bufferBytes.Count + 1));
                encodedBytes.AddRange(bufferBytes);
                encodedBytes.Add(prev);
            }

            return encodedBytes;
        }

        public IEnumerable<byte> Decode()
        {
            List<byte> decodedBytes = new();
            var sr = new BinaryReader(_dataStream);

            while (sr.BaseStream.Position != sr.BaseStream.Length)
            {
                byte flagL = sr.ReadByte();
                if (flagL >= 128)
                {
                    decodedBytes.AddRange(Enumerable.Repeat(sr.ReadByte(), flagL - 128));
                    continue;
                }
                for (int i = 0; i < flagL; i++)
                    decodedBytes.Add(sr.ReadByte());

            }
            return decodedBytes;
        }

        public void Dispose()
            => _dataStream?.Dispose();
    }
}
