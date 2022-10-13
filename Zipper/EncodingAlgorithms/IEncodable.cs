using System;
using System.Collections.Generic;
using System.IO;

namespace Zipper.EncodingAlgorithms
{
    public interface IEncodable : IDisposable
    {
        /// <summary>Кодирует данные из потока определенным алгоритмом.
        /// <example>
        /// Пример использования: <code>algorithm.Encode(File.OpenRead(f.FullName),f.Length)</code>
        /// </example>
        /// </summary>
        /// <param name="dataStream">Поток, с которого будут считываться данные для кодирования.</param>
        /// <param name="originalBytesCount">Количество байт, которое будет закодировано.</param>
        /// <returns>Результирующие закодированные байты.</returns>
        /// <remarks>Следует использовать как генератор для предотвращения <see cref="OutOfMemoryException"/> или <see cref="IndexOutOfRangeException"/></remarks>
        IEnumerable<byte> Encode(in Stream dataStream, long originalBytesCount);

        /// <summary>Декодирует данные из потока определенным алгоритмом.</summary>
        /// <param name="encodedDataStream">Поток, из которого будут декодироваться данные.</param>
        /// <param name="encodedBytesCount">Количество байт, которе будет декодировано.</param>
        /// <param name="originalBytesCount">Количество байт, которое будет закодировано.</param>
        /// <returns>Результирующие декодированные байты.</returns>
        /// <remarks>Следует использовать как генератор для предотвращения <see cref="OutOfMemoryException"/> или <see cref="IndexOutOfRangeException"/></remarks>
        IEnumerable<byte> Decode(in Stream encodedDataStream, long encodedBytesCount, long? originalBytesCount = null);
    }
}
