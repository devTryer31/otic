using System.Collections.Generic;
using System.Linq;

namespace Zipper
{
    public static class BitHelper
    {
        public static IEnumerable<bool> ToBits(this byte b)
        {
            List<bool> ans = new();
            for (int i = 0; i < 8; i++)
            {
                ans.Add((b & 128) / 128 == 1);
                b <<= 1;
            }
            return ans;
        }

        public static IEnumerable<byte> ToBytes(this List<bool> bits)
        {
            //Записываем все целые байты.
            List<byte> ans = new(bits.Count / 8 + 1);
            byte currentByte = 0;
            for (int i = 0; i < bits.Count; i++)
            {
                currentByte += (byte)(bits[i] ? 1 : 0);
                if (i % 7 == 0 && i != 0)
                {
                    ans.Add(currentByte);
                    currentByte = 0;
                }
                currentByte <<= 1;
            }

            //Записываем последний нецелый, если он будет.
            int tailBitsCnt = 7 - bits.Count % 8;
            while (tailBitsCnt-- > 0)
                currentByte <<= 1;
            ans.Add(currentByte);

            return ans;
        }

        public static byte GetByteFromBits(this IEnumerable<bool> bits)
        {
            byte ans = (byte)(bits.First() ? 1 : 0);
            foreach (bool bit in bits.Skip(1))
            {
                ans <<= 1;
                ans += (byte)(bit ? 1 : 0);
            }
            return ans;
        }
    }
}
