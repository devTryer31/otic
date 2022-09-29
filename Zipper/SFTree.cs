﻿using System;
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
        private Dictionary<byte, List<bool>> _bytesCode = new();

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
    }
}
