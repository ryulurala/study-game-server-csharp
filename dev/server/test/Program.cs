using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            // IP 주소
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];

            // Port 번호
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 문지기(TCP): Listen Socket 초기화
            Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


            try
            {
                // 문지기 교육: Bind
                listenSocket.Bind(endPoint);

                // 영업시작: Listen
                // backlog: 최대 대기수, 몇 명이 대기할 지... backlog를 초과하면 모두 fail
                listenSocket.Listen(10);    // Accept() 되기 전까지 10명 대기

                while (true)
                {
                    Console.WriteLine("Listening...");

                    // 손님 안내: Accept()
                    Socket clientSocket = listenSocket.Accept();    // Blocking: 다음 단계 불가

                    // 메시지 받기: Receive()
                    byte[] recvBuff = new byte[1024];
                    int recvBytes = clientSocket.Receive(recvBuff);
                    string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
                    Console.WriteLine($"[From Client] {recvData}");

                    // 메시지 보내기: Send()
                    byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server !");
                    clientSocket.Send(sendBuff);    // Blocking: 다음 단계 불가

                    // 손님 보내기: Close()
                    clientSocket.Shutdown(SocketShutdown.Both);   // 신뢰성(TCP)
                    clientSocket.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
