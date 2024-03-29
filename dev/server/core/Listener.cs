using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace core
{
    public class Listener
    {
        Socket _listenSocket;
        Func<Session> _sessionFactory;

        public void Listen(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _sessionFactory += sessionFactory;

            _listenSocket.Bind(endPoint);
            _listenSocket.Listen(backlog);

            for (int i = 0; i < register; i++)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
                RegisterAccept(args);   // Accept() 이벤트 등록
            }
        }
        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // 기존에 있던 클라이언트 소켓 clear
            args.AcceptSocket = null;

            // Accept()를 요청/등록
            bool pending = _listenSocket.AcceptAsync(args);

            if (!pending)
            {
                //팬딩 없이 완료됨.
                OnAcceptCompleted(null, args);
            }
        }
        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 콜백으로 실행
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }

            // 다음 클라이언트를 위한 등록
            RegisterAccept(args);
        }
    }
}
