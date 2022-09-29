using System;
using System.Collections.Generic;
using System.Linq;

namespace Zipper
{
    using FrequencyList = List<(byte Byte, long Count)>;
    public class SFTree
    {

        private Dictionary<byte, long> _bytesFrequency = new();
        private Dictionary<byte, List<bool>> _bytesCode = new();
        private readonly byte[] _bytes;

        public SFTree(byte[] bytes)
        {
            foreach (var b in bytes)
            {
                if (!_bytesFrequency.ContainsKey(b))
                    _bytesFrequency.Add(b, 0L);
                ++_bytesFrequency[b];
            }

            var sortedBytesFrequency = _bytesFrequency
                .OrderByDescending(p => p.Value)
                .Select(p => (p.Key, p.Value))
                .ToList();

            BuildTree(sortedBytesFrequency);

#if DEBUG
            foreach (var p in _bytesCode)
                System.Diagnostics.Debug.WriteLine($"{p.Key} : [{string.Join("", p.Value.Select(b => b ? 1 : 0))}]");
#endif
            _bytes = bytes;
        }

        public IEnumerable<byte> GetTreeInBytes()
        {
            //Создадим лист битов, который потом будем переводить в байты.
            List<bool> tree = new(_bytesCode.Count * (8 + 2));
            foreach (var (bt, code) in _bytesCode)
            {
                var byteBits = bt.ToBits();
                tree.AddRange(byteBits);

                byte codeLen = (byte)code.Count;//У нас не будет кода, большего byte.MaxValue
                tree.AddRange(codeLen.ToBits());

                tree.AddRange(code);
            }

            return tree.ToBytes();
        }

        private void BuildTree(FrequencyList bytesPart)
        {
            if (bytesPart.Count == 1)
                return;

            long lSum = 0, rSum = 0;
            int l = -1, r = bytesPart.Count;
            while (true)
            {
                if (lSum < rSum)
                    lSum += bytesPart[++l].Count;
                else
                    rSum += bytesPart[--r].Count;
                if (r - l == 1)
                    break;
            }

            FrequencyList lhs = bytesPart.GetRange(0, l + 1);
            FrequencyList rhs = bytesPart.GetRange(r, bytesPart.Count - r);

            foreach (var t in lhs)
            {
                if (!_bytesCode.ContainsKey(t.Byte))
                    _bytesCode.Add(t.Byte, new List<bool>());

                _bytesCode[t.Byte].Add(false);
            }

            foreach (var t in rhs)
            {
                if (!_bytesCode.ContainsKey(t.Byte))
                    _bytesCode.Add(t.Byte, new List<bool>());

                _bytesCode[t.Byte].Add(true);
            }

            BuildTree(lhs);
            BuildTree(rhs);
        }

        public IEnumerable<byte> EncodeBytes()
        {
            List<bool> bitsEncoded = new(_bytes.Length * 2);
            foreach (var b in _bytes)
                bitsEncoded.AddRange(_bytesCode[b]);
            
            return bitsEncoded.ToBytes();
        }

        public List<byte> DecodeBytes(byte[] data, int startBytesLen)
        {
            var codes = _bytesCode.Values.ToList();
            List<List<bool>> prediction = codes;
            List<byte> decoded = new();

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
                        decoded.Add(_bytesCode.First(p => p.Value == prediction[0]).Key);
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
