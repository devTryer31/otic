using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zipper.EncodingAlgorithms.Utils;

namespace Zipper.EncodingAlgorithms.Core
{
    public static class RLE
    {
        private const byte _MaxBufferCount = 127;

        /// <summary>Операция повторения байта.</summary>
        /// <param name="b">Повторяемый байт.</param>
        /// <param name="count">Сколько раз повторить.</param>
        /// <returns>Префикс количества и сам байт.</returns>
        private static IEnumerable<byte> ReturnRepeat(byte b, int count)
        {
            while (count > _MaxBufferCount)
            {
                yield return 128 + _MaxBufferCount;
                yield return b;
                count -= _MaxBufferCount;
            }
            yield return (byte)(128 + count);
            yield return b;
        }

        /// <summary>Высвобождение байтов, если буфер переполнен.</summary>
        /// <returns>Байты с префиксом количества, если буфер переполнен.</returns>
        private static IEnumerable<byte> HandleBufferOverflow(Queue<byte> buffer)
        {
            while (buffer.Count > _MaxBufferCount)
            {
                yield return _MaxBufferCount;
                byte cnt = _MaxBufferCount;
                while (cnt-- > 0)
                    yield return buffer.Dequeue();
            }
        }

        /// <summary>Вставка в буфер с последующей обработкой переполнения.</summary>
        /// <param name="b">Байт, который следует вставить в буфер.</param>
        /// <returns>Байты из буфера с префиксом количества, если он переполнился после вставки.</returns>
        private static IEnumerable<byte> InsertInBuffer(in Queue<byte> buffer, byte b)
        {
            buffer.Enqueue(b);
            return HandleBufferOverflow(buffer);
        }

        /// <summary>Высвобождает буфер с учетом переполнения.</summary>
        /// <returns>Байты из буфера с префиксами количества.</returns>
        private static IEnumerable<byte> DumpBuffer(Queue<byte> buffer)
        {
            foreach (byte b in HandleBufferOverflow(buffer))
                yield return b;

            yield return (byte)buffer.Count;//Will be < 127 after HandleBufferOverflow.

            while (buffer.TryDequeue(out byte b))
                yield return b;
        }

        /// <summary>Кодирует все данные из потока <paramref name="dataStream"/> алгоритмом RLE</summary>
        /// <param name="dataStream">Поток, данные из которго будут кодироваться.</param>
        /// <returns>Перечисление закодированых байтов.</returns>
        public static IEnumerable<byte> Encode(Stream dataStream)
        {
            if (dataStream.IsEndOfStream())
                yield break;

            var sr = new BinaryReader(dataStream);
            Queue<byte> bufferBytes = new(127 + 1);
            int countSameBytes = 1;
            byte prev = sr.ReadByte();
            while (!dataStream.IsEndOfStream())
            {
                byte current = sr.ReadByte();
                if (prev == current)
                {
                    ++countSameBytes;
                    if (dataStream.IsEndOfStream())//We need handle case when last is repeatable.
                        goto SKIP_CONTINUE;
                    continue;
                }
            SKIP_CONTINUE:;

                if (countSameBytes < 3)//If no need to compress.
                {
                    while (countSameBytes-- > 0)
                        foreach (byte b in InsertInBuffer(bufferBytes, prev))
                            yield return b;
                }
                else //Handle repeat case.
                {
                    //If the buffer not empty we need to dump it.
                    foreach (byte b in DumpBuffer(bufferBytes))
                        yield return b;
                    foreach (byte b in ReturnRepeat(prev, countSameBytes))
                        yield return b;
                }
                if (dataStream.IsEndOfStream() && prev != current)//The case, when last byte in stream 
                {                                                 //are not repeatable.
                    foreach (byte b in InsertInBuffer(bufferBytes, current))
                        yield return b;

                    break;
                }
                else
                {
                    prev = current;
                    countSameBytes = 1;
                }
            }
            foreach (byte b in DumpBuffer(bufferBytes))
                yield return b;
        }

        public static IEnumerable<byte> Decode(Stream dataStream, long encodedBytesCount)
        {
            long startStreamPosition = dataStream.Position;
            var sr = new BinaryReader(dataStream);

            while (dataStream.Position - startStreamPosition != encodedBytesCount)
            {
                byte flagL = sr.ReadByte();
                if (flagL >= 128)
                {
                    foreach (byte b in Enumerable.Repeat(sr.ReadByte(), flagL - 128))
                        yield return b;
                    continue;
                }
                for (int i = 0; i < flagL; i++)
                    yield return sr.ReadByte();
            }
        }
    }
}
