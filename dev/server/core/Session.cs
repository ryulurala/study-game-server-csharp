using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace core
{
    public abstract class PacketSession : Session
    {
        public static readonly int HeadSize = 2;
        public sealed override int OnRecv(ArraySegment<byte> buffer)
        {
            // [size(2)][packetId(2)][...][size(2)][packetId(2)][...]
            int processLen = 0;

            // Parsing packet
            while (true)
            {
                // 최소 헤더 파싱 여부
                if (buffer.Count < HeadSize)
                    break;

                // Packet이 완전체로 도착했는지 확인
                ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
                if (buffer.Count < dataSize)
                    break;

                // Packet 조립
                // [size(2)][packetId(2)][...] 넘겨줌.
                OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

                processLen += dataSize;
                buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
            }

            return processLen;
        }
        public abstract void OnRecvPacket(ArraySegment<byte> buffer);
    }

    public abstract class Session
    {
        Socket _socket;     // 클라이언트 소켓(대리자)
        int _disconnected = 0;

        RecvBuffer _recvBuffer = new RecvBuffer(1024);
        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();    // 재사용
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();    // 재사용
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        object _lock = new object();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract int OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        void Clear()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _pendingList.Clear();
            }
        }

        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            ResgisterRecv();
        }
        public void Send(ArraySegment<byte> sendBuff)
        {
            lock (_lock)
            {
                // 쓰레드 하나씩
                _sendQueue.Enqueue(sendBuff);

                if (_pendingList.Count == 0) RegisterSend();
            }
        }
        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) return;

            OnDisconnected(_socket.RemoteEndPoint);

            // 손님 보내기: Close()
            _socket.Shutdown(SocketShutdown.Both);   // 신뢰성(TCP)
            _socket.Close();

            Clear();
        }

        #region 네트워크 통신
        void RegisterSend()
        {
            if (_disconnected == 1)
                return;

            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> buff = _sendQueue.Dequeue();
                _pendingList.Add(buff);
            }
            _sendArgs.BufferList = _pendingList;

            try
            {
                bool pending = _socket.SendAsync(_sendArgs);
                if (pending == false)
                    OnSendCompleted(null, _sendArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterSend Failed {e}");
            }
        }
        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        _sendArgs.BufferList = null;
                        _pendingList.Clear();
                        OnSend(_sendArgs.BytesTransferred);


                        if (_sendQueue.Count > 0) RegisterSend();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e.ToString()}");
                    }
                }
                else
                {
                    // TODO Disconnect
                    Disconnect();
                }
            }
        }
        void ResgisterRecv()
        {
            if (_disconnected == 1)
                return;

            _recvBuffer.Clean();
            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = _socket.ReceiveAsync(_recvArgs);
                if (pending == false)
                {
                    OnRecvCompleted(null, _recvArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"RegisterRecv Failed {e}");
            }
        }
        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                // 성공적으로 Data를 가져옴
                try
                {
                    // Write 커서 이동
                    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // 컨텐츠단으로 데이터를 넘겨줌
                    int processLen = OnRecv(_recvBuffer.ReadSegment);
                    if (processLen < 0 || _recvBuffer.DataSize < processLen)
                    {
                        Disconnect();
                        return;
                    }

                    // Read 커서 이동
                    if (_recvBuffer.OnRead(processLen) == false)
                    {
                        Disconnect();
                        return;
                    }

                    ResgisterRecv();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecvCompleted Failed {e.ToString()}");
                }
            }
            else
            {
                // TODO Disconnect
                Disconnect();
            }
        }
        #endregion
    }
}