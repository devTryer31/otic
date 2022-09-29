using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zipper
{
    using FrequencyList = List<(byte Byte, long Count)>;
    public class SFTree
    {

        private Dictionary<byte, long> _bytesFrequency = new();
        private Dictionary<byte, List<bool>> _bytesCode = new();// коды

        public SFTree(byte[] bytes) // таблица значения и код
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

            EncodBytes(sortedBytesFrequency);


            foreach (var p in _bytesCode)
            {
                System.Diagnostics.Debug.WriteLine($"{p.Key} : [{string.Join("", p.Value.Select(b => b ? 1 : 0))}]");
            }
        }

        public void EncodBytes(FrequencyList bytesPart)
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

            EncodBytes(lhs);
            EncodBytes(rhs);
        }

        public List<byte> DecodeBytes(byte[] data, int startBytesLen)
        {
            var codes = _bytesCode.Values.ToList();
            List<List<bool>> prediction = codes;
            List<byte> decoded = new();
            //File.ReadAllBytes(@"C:\Users\SOLOVEV\source\repos\devTryer31\otic\Zipper\123.txt");
            int searchIdx = 0;
            for (int i = 0; i < data.Length; i++)
            {
                byte current = data[i];

                bool bit = false;
                for (int k = 0; k < 8; k++) //1 
                {
                    //System.Diagnostics.Debug.WriteLine("current before " + current.ToString());
                    bit = (current & 128) / 128 == 1;
                    //System.Diagnostics.Debug.WriteLine("k " + k.ToString());
                    //System.Diagnostics.Debug.WriteLine("bit " + bit.ToString());
                    current <<= 1;
                    //System.Diagnostics.Debug.WriteLine("current after " + current.ToString());
                    List<List<bool>> buff = new();

                    for (int j = 0; j < prediction.Count; j++)
                    {

                        if (bit == prediction[j][searchIdx])
                        {
                            buff.Add(prediction[j]);
                        }
                    }
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
            System.Diagnostics.Debug.WriteLine(codes[0]);

            throw new ArgumentException("Невернный фромат входных байтов");
        }
    }
}
