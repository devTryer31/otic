using System;
using System.Collections.Generic;
using System.IO;

namespace Zipper.EncodingAlgorithms.Utils
{
    static public class BufferedReadingData
    {
        /// <summary>Предоставляет буферизированное чтение байтов в виде генератора.</summary>
        /// <param name="sr">Откуда будет производиться считывание.</param>
        /// <param name="countToRead">Сколько нужно считать.</param>
        /// <returns>Считанные байты.</returns>
        /// <exception cref="ArgumentNullException">Если <paramref name="sr"/> является null</exception>
        public static IEnumerable<byte> GetBytesData(BinaryReader sr, long countToRead)
        {
            if (sr is null)
                throw new ArgumentNullException(nameof(sr));

            const int toRead = 64_000_000;
            while (countToRead > 0)
            {
                foreach (byte b in sr.ReadBytes((int)Math.Min(toRead, countToRead)))
                    yield return b;
                countToRead -= toRead;
            }
        }
    }
}
