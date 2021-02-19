using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace core
{
    abstract public class Session
    {
        Socket _socket;     // 클라이언트 소켓(대리자)
        int _disconnected = 0;
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();    // 재사용
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();    // 재사용
        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
        object _lock = new object();

        public abstract void OnConnected(EndPoint endPoint);
        public abstract void OnRecv(ArraySegment<byte> buffer);
        public abstract void OnSend(int numOfBytes);
        public abstract void OnDisconnected(EndPoint endPoint);

        public void Start(Socket socket)
        {
            _socket = socket;

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

            _recvArgs.SetBuffer(new byte[1024], 0, 1024);

            ResgisterRecv();
        }
        public void Send(byte[] sendBuff)
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
        }

        #region 네트워크 통신
        void RegisterSend()
        {
            while (_sendQueue.Count > 0)
            {
                byte[] buff = _sendQueue.Dequeue();
                _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
            }
            _sendArgs.BufferList = _pendingList;

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
            {
                OnSendCompleted(null, _sendArgs);
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
            bool pending = _socket.ReceiveAsync(_recvArgs);
            if (!pending)
            {
                OnRecvCompleted(null, _recvArgs);
            }
        }
        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                // 성공적으로 Data를 가져옴
                try
                {
                    OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));


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