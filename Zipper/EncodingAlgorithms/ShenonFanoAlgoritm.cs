using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zipper.EncodingAlgorithms.Core;
using Zipper.EncodingAlgorithms.Utils;

namespace Zipper.EncodingAlgorithms
{
    public class ShenonFanoAlgoritm : IEncodable
    {
        private SFTree? _sFTree = null;

        public IEnumerable<byte> Encode(in Stream dataStream, long originalBytesCount)
        {
            _sFTree = new SFTree(dataStream);

            byte[] freqBytes = _sFTree.GetFrequenciesInBytes();
            return BitConverter.GetBytes(freqBytes.Length).Concat(freqBytes).Concat(_sFTree.EncodeBytes());
        }

        public IEnumerable<byte> Decode(in Stream encodedDataStream, long encodedBytesCount, long? originalBytesCount)
        {
            if (originalBytesCount is null)
                throw new ArgumentNullException(nameof(originalBytesCount));

            var sr = new BinaryReader(encodedDataStream);
            int freqsLen = sr.ReadInt32();
            encodedBytesCount -= sizeof(int);
            var freqsInBytes = sr.ReadBytes(freqsLen);
            encodedBytesCount -= freqsLen;
            return SFTree.DecodeBytes(BufferedReadingData.GetBytesData(sr, encodedBytesCount), originalBytesCount.Value, freqsInBytes);
        }

        public void Dispose()
        {
            _sFTree?.Dispose();
        }
    }
}
