using System.Collections.Generic;
using System.IO;
using Zipper.EncodingAlgorithms.Utils;

namespace Zipper.EncodingAlgorithms
{
    public class NoneAlgorithm : IEncodable
    {
        private Stream? _dataStream;
        private Stream? _encodedDataStream;

        public IEnumerable<byte> Encode(in Stream dataStream, long originalBytesCount)
        {
            _dataStream = dataStream;
            var br = new BinaryReader(dataStream);
            return BufferedReadingData.GetBytesData(br, originalBytesCount);
        }
        public IEnumerable<byte> Decode(in Stream encodedDataStream, long encodedBytesCount, long? originalBytesCount = null)
        {
            _encodedDataStream = encodedDataStream;
            var br = new BinaryReader(encodedDataStream);
            return BufferedReadingData.GetBytesData(br, encodedBytesCount);
        }

        public void Dispose()
        {
            _dataStream?.Dispose();
            _encodedDataStream?.Dispose();
        }
    }
}
