using System.Collections.Generic;
using System.IO;
using Zipper.EncodingAlgorithms.Core;

namespace Zipper.EncodingAlgorithms
{
    public class RLEAlgorithm : IEncodable
    {
        private Stream? _dataStream;
        private Stream? _encodedDataStream;

        public IEnumerable<byte> Encode(in Stream dataStream, long originalBytesCount)
        {
            _dataStream = dataStream;
            return RLE.Encode(dataStream);
        }

        public IEnumerable<byte> Decode(in Stream encodedDataStream, long encodedBytesCount, long? originalBytesCount = null)
        {
            _encodedDataStream = encodedDataStream;
            return RLE.Decode(encodedDataStream, encodedBytesCount);
        }

        public void Dispose(){
            _dataStream?.Dispose();
            _encodedDataStream?.Dispose();
        }
    }
}
