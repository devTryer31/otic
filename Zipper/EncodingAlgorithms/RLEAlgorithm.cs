using System.Collections.Generic;
using System.IO;

namespace Zipper.EncodingAlgorithms
{
    public class RLEAlgorithm : IEncodable
    {
        private RLE? _rLE;

        public IEnumerable<byte> Encode(in Stream dataStream, long originalBytesCount)
        {
            _rLE ??= new RLE(dataStream);
            return _rLE.Encode();
        }

        public IEnumerable<byte> Decode(in Stream encodedDataStream, long encodedBytesCount, long? originalBytesCount = null)
        {
            _rLE ??= new RLE(encodedDataStream);
            return _rLE.Decode();
        }

        public void Dispose()
            => _rLE?.Dispose();
    }
}
