using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Zipper
{
    using FrequencyList = List<(byte Byte, byte Count)>;
    using BytesCodes = Dictionary<byte, List<bool>>;

    public class SFTree : IDisposable
    {

        private Dictionary<byte, long> _bytesFrequency = new();
        private readonly BytesCodes _bytesCode;
        private readonly FrequencyList _sortedBytesFrequency;
        private readonly Stream _dataStream;

        public SFTree(Stream dataStream)
        {
            var sr = new BinaryReader(dataStream);
            while (!sr.BaseStream.IsEndOfStream())
            {
                byte b = sr.ReadByte();
                if (!_bytesFrequency.ContainsKey(b))
                    _bytesFrequency.Add(b, 0L);
                ++_bytesFrequency[b];
            }
            dataStream.Flush();
            dataStream.Position = 0;

            _sortedBytesFrequency = _bytesFrequency
                .Select(p => (Byte: p.Key, Count: NormalizeLongLinear(p.Value)))
                .OrderByDescending(i => i.Count)
                .ThenBy(p => p.Byte)
                .ToList();

            _bytesCode = BuildTree(_sortedBytesFrequency);
            _dataStream = dataStream;

#if DEBUG
            foreach (var p in _bytesCode)
                System.Diagnostics.Debug.WriteLine($"{p.Key} : [{string.Join("", p.Value.Select(b => b ? 1 : 0))}] len = {p.Value.Count}");
#endif
        }

        private byte NormalizeLongLinear(long source)
            => (byte)(1 + (source - 1) * (byte.MaxValue - 1) / (_bytesFrequency.Values.Max() - 1));

        public byte[] GetFrequenciesInBytes()
        {
            byte[] ans = new byte[byte.MaxValue + 1];
            for (int i = byte.MinValue; i <= byte.MaxValue; i++)
            {
                byte b = (byte)i;

                if (!_bytesFrequency.ContainsKey(b))
                    ans[i] = 0;
                else
                    ans[i] = NormalizeLongLinear(_bytesFrequency[b]);
            }
            return ans;
        }

        private static BytesCodes GetTreeFromFrequenciesBytes(byte[] frequencies)
        {
            using var sr = new BinaryReader(new MemoryStream(frequencies));
            FrequencyList decodedFrequencies = new(byte.MaxValue);

            //Читаем частотности байтов.
            for (int i = 0; i <= byte.MaxValue; i++)
            {
                byte bi = (byte)i;

                byte b = sr.ReadByte();
                if (b != 0)
                    decodedFrequencies.Add((bi, b));
            }

            return BuildTree(decodedFrequencies.OrderByDescending(p => p.Count)
                .ThenBy(p => p.Byte)
                .ToList());
        }

        private static BytesCodes BuildTree(in FrequencyList bytesPart, BytesCodes? bytesCodes = null)
        {
            if (bytesCodes is null)
                bytesCodes = new();

            if (bytesPart.Count <= 1)
            {
                if (bytesCodes.Count == 0)
                    bytesCodes.Add(bytesPart.First().Byte, new List<bool>() { false });
                return bytesCodes;
            }

            long lSum = 0, rSum = 0;
            int l = -1, r = bytesPart.Count;
            while (true)
            {
                if (lSum < rSum)
                    lSum += bytesPart[++l].Count;
                else
                    rSum += bytesPart[--r].Count;
                if (r - l == 1)
                {
                    if (r == bytesPart.Count)
                        r--;
                    break;
                }
            }

            FrequencyList lhs = bytesPart.GetRange(0, l + 1);
            FrequencyList rhs = bytesPart.GetRange(r, bytesPart.Count - r);

            foreach (var t in lhs)
            {
                if (!bytesCodes.ContainsKey(t.Byte))
                    bytesCodes.Add(t.Byte, new List<bool>());

                bytesCodes[t.Byte].Add(false);
            }

            foreach (var t in rhs)
            {
                if (!bytesCodes.ContainsKey(t.Byte))
                    bytesCodes.Add(t.Byte, new List<bool>());

                bytesCodes[t.Byte].Add(true);
            }

            BuildTree(lhs, bytesCodes);
            BuildTree(rhs, bytesCodes);

            return bytesCodes;
        }

        public IEnumerable<byte> EncodeBytes()
        {
            var sr = new BinaryReader(_dataStream);
            Queue<bool> buffer = new(8 * 2);
            while (!sr.BaseStream.IsEndOfStream())
            {
                var code = _bytesCode[sr.ReadByte()];
                code.ForEach(buffer.Enqueue);
                while (buffer.Count >= 8)
                {
                    yield return buffer.ToSingleByte();
                    byte cnt = 8;
                    while (cnt-- > 0)
                        buffer.Dequeue();
                }
            }
            _dataStream.Flush();
            _dataStream.Position = 0;
            yield return buffer.ToSingleByte();
        }

        public static IEnumerable<byte> DecodeBytes(IEnumerable<byte> data, long startBytesLen, byte[] frequencies)
        {
            var bytesCode = GetTreeFromFrequenciesBytes(frequencies);

#if DEBUG
            foreach (var p in bytesCode)
                System.Diagnostics.Debug.WriteLine($"{p.Key} : [{string.Join("", p.Value.Select(b => b ? 1 : 0))}] len = {p.Value.Count}");
#endif

            List<List<bool>> prediction = bytesCode.Values.OrderBy(arr => arr.Count).ToList();

            var revertedBytesCode = bytesCode.ToDictionary(x => x.Value, x => x.Key, new LigthComparator<List<bool>>((x, y) => x.SequenceEqual(y)
            , x =>
            {
                int sum = 0;
                for (int i = 0; i < x.Count; i++)
                    sum += (int)Math.Pow(2, i) * (x[i] ? 1 : 0);
                return sum;
            }));

            var minLen = prediction[0].Count;
            int currentLen = minLen;

            currentLen = minLen;
            List<bool> currentBits = new();

            foreach(byte b in data)
            {
                byte current = b;

                bool bit = false;
                for (int k = 0; k < 8; k++)
                {
                    bit = (current & 128) / 128 == 1;
                    current <<= 1;
                    currentBits.Add(bit);

                    if (currentBits.Count == currentLen)
                    {
                        if (revertedBytesCode.ContainsKey(currentBits))
                        {
                            yield return revertedBytesCode[currentBits];
                            --startBytesLen;
                            if(startBytesLen == 0)
                                goto GOOD_END;
                            currentBits.Clear();
                            currentLen = minLen - 1;
                        }
                        currentLen++;
                    }
                }
            }
            throw new ArgumentException("Невернный фромат входных байтов");
            GOOD_END:;
        }

        public void Dispose()
            => _dataStream.Dispose();
    }
}
