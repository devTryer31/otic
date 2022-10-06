using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Zipper
{
    using FrequencyList = List<(byte Byte, byte Count)>;
    using BytesCodes = Dictionary<byte, List<bool>>;

    public class SFTree
    {

        private Dictionary<byte, long> _bytesFrequency = new();
        private readonly BytesCodes _bytesCode;
        private readonly byte[] _bytes;
        private readonly FrequencyList _sortedBytesFrequency;

        public SFTree(byte[] bytes)
        {
            foreach (var b in bytes)
            {
                if (!_bytesFrequency.ContainsKey(b))
                    _bytesFrequency.Add(b, 0L);
                ++_bytesFrequency[b];
            }

            _sortedBytesFrequency = _bytesFrequency
                .Select(p => (Byte: p.Key, Count: NormalizeLongLinear(p.Value)))
                .OrderByDescending(i => i.Count)
                .ThenBy(p => p.Byte)
                .ToList();

            _bytesCode = BuildTree(_sortedBytesFrequency);

#if DEBUG
            foreach (var p in _bytesCode)
                System.Diagnostics.Debug.WriteLine($"{p.Key} : [{string.Join("", p.Value.Select(b => b ? 1 : 0))}] len = {p.Value.Count}");
#endif
            _bytes = bytes;
        }

        private byte NormalizeLongLinear(long source)
            => (byte)(1 + (source - 1) * (byte.MaxValue - 1) / (_bytesFrequency.Values.Max() - 1));

        public List<byte> GetFrequenciesInBytes()
        {
            List<byte> ans = new();
            for (int i = byte.MinValue; i <= byte.MaxValue; i++)
            {
                byte b = (byte)i;

                if (!_bytesFrequency.ContainsKey(b))
                    ans.Add(0);
                else
                {
                    byte normalizedFreq = NormalizeLongLinear(_bytesFrequency[b]);
                    ans.Add(normalizedFreq);
                }
            }
            return ans;
        }

        private static BytesCodes GetTreeFromBytes(List<byte> frequencies)
        {
            var frequenciesArr = frequencies.ToArray();
            using var sr = new BinaryReader(new MemoryStream(frequenciesArr));
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
                return bytesCodes;

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
            List<bool> bitsEncoded = new(_bytes.Length * 2);
            foreach (var b in _bytes)
                bitsEncoded.AddRange(_bytesCode[b]);

            return bitsEncoded.ToBytes();
        }

        public static List<byte> DecodeBytes(byte[] data, int startBytesLen, List<byte> frequencies)
        {
            var bytesCode = GetTreeFromBytes(frequencies);

#if DEBUG
            foreach (var p in bytesCode)
                System.Diagnostics.Debug.WriteLine($"{p.Key} : [{string.Join("", p.Value.Select(b => b ? 1 : 0))}] len = {p.Value.Count}");
#endif

            var codes = bytesCode.Values.ToList();
            List<List<bool>> prediction = codes.OrderBy(arr => arr.Count).ToList();
            List<byte> decoded = new(startBytesLen);

            var revertedBytesCode = bytesCode.ToDictionary(x => x.Value, x => x.Key, new LigthComparator<List<bool>>((x, y) => x.SequenceEqual(y)
            , x =>
            {
                int sum = 0;
                for (int i = 0; i < x.Count; i++)
                    sum += (int)Math.Pow(2, i) * (x[i] ? 1 : 0);
                return sum;
            }));

            var minLen = prediction[0].Count();
            int currentLen = minLen;

            currentLen = minLen;
            List<bool> currentBits = new();

            for (int i = 0; i < data.Length; i++)
            {
                byte current = data[i];

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
                            decoded.Add(revertedBytesCode[currentBits]);
#if DEBUG
                            System.Diagnostics.Debug.WriteLine(decoded.Last());
#endif
                            if (decoded.Count == startBytesLen)
                                return decoded;
                            currentBits.Clear();
                            currentLen = minLen - 1;
                        }
                        currentLen++;
                    }
                }
            }
            throw new ArgumentException("Невернный фромат входных байтов");
        }
    }
}
