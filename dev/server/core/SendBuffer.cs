using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace core
{
    public class SendBufferHelper
    {
        // Thread의 고유한 전역 공간
        public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => null);
        public static int ChunkSize { get; set; } = 65535 * 100;

        public static ArraySegment<byte> Open(int reserveSize)
        {
            if (CurrentBuffer.Value == null)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            if (CurrentBuffer.Value.FreeSize < reserveSize)
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }

    }

    public class SendBuffer
    {
        // [u][][][][][][][]
        byte[] _buffer;     // buffer
        int _usedSize = 0;  // used cursor

        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }

        // Property: 남은 공간 크기
        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        public ArraySegment<byte> Open(int reserveSize)
        {
            if (reserveSize > FreeSize)
                return default(ArraySegment<byte>);

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }

        public ArraySegment<byte> Close(int usedSize)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
            _usedSize += usedSize;

            return segment;
        }
    }
}