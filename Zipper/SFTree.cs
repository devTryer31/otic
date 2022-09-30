using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Zipper
{
    using FrequencyList = List<(byte Byte, long Count)>;
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
                .OrderByDescending(p => p.Value)
                .ThenBy(p => p.Key)
                .Select(p => (p.Key, p.Value))
                .ToList();

            _bytesCode = BuildTree(_sortedBytesFrequency);

#if DEBUG
            foreach (var p in _bytesCode)
                System.Diagnostics.Debug.WriteLine($"{p.Key} : [{string.Join("", p.Value.Select(b => b ? 1 : 0))}] len = {p.Value.Count} bit count->{_bytesFrequency[p.Key]}");
#endif
            _bytes = bytes;
        }

        public List<byte> GetFrequenciesInBytes()
        {
            List<byte> ans = new(4 + _sortedBytesFrequency.Count * (8 + 1));
            //Добавляем количество частотночтей.
            ans.AddRange(BitConverter.GetBytes(_sortedBytesFrequency.Count));
            foreach (var (b, freq) in _sortedBytesFrequency)
            {
                //Добавляем сам кодируемый байт.
                ans.Add(b);
                //Добавляем саму частотность.
                ans.AddRange(BitConverter.GetBytes(freq));
            }
            return ans;
        }

        private static BytesCodes GetTreeFromBytes(List<byte> frequencies)
        {
            var frequenciesArr = frequencies.ToArray();
            using var sr = new BinaryReader(new MemoryStream(frequenciesArr));
            //Читаем количество частотночтей.
            int frCount = sr.ReadInt32();

            FrequencyList decodedFrequencies = new(frCount);

            //Читаем кодируемые байы и их частотность.
            for (int i = 0; i < frCount; i++)
                decodedFrequencies.Add((sr.ReadByte(), sr.ReadInt64()));
            
            return BuildTree(decodedFrequencies);
        }

        private static BytesCodes BuildTree(in FrequencyList bytesPart, BytesCodes? bytesCodes = null)
        {
            if(bytesCodes is null)
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

            var codes = bytesCode.Values.ToList();
            List<List<bool>> prediction = codes;
            List<byte> decoded = new(startBytesLen);

            int searchIdx = 0;
            for (int i = 0; i < data.Length; i++)
            {
                byte current = data[i];

                bool bit = false;
                for (int k = 0; k < 8; k++)
                {
                    bit = (current & 128) / 128 == 1;
                    current <<= 1;
                    List<List<bool>> buff = new();

                    for (int j = 0; j < prediction.Count; j++)
                        if (bit == prediction[j][searchIdx])
                            buff.Add(prediction[j]);

                    searchIdx++;

                    prediction = buff;

                    if (prediction.Count == 1)
                    {
                        decoded.Add(bytesCode.First(p => Enumerable.SequenceEqual(p.Value, prediction[0])).Key);
                        if (decoded.Count == startBytesLen)
                            return decoded;
                        prediction = codes;
                        searchIdx = 0;
                    }
                }
            }

            throw new ArgumentException("Невернный фромат входных байтов");
        }
    }
}
