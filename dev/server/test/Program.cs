using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using core;

namespace test
{
    class Knight
    {
        public int hp;
        public int attack;
    }
    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Onconnected: {endPoint}");

            Knight knight = new Knight() { hp = 100, attack = 10 };

            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            byte[] buffer1 = BitConverter.GetBytes(knight.hp);
            byte[] buffer2 = BitConverter.GetBytes(knight.attack);

            Array.Copy(buffer1, 0, openSegment.Array, openSegment.Offset, buffer1.Length);
            Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer1.Length, buffer2.Length);
            ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer1.Length + buffer2.Length);

            Send(sendBuff);

            Thread.Sleep(1000);

            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client]\n {recvData}");
            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred Byte: {numOfBytes}\n");
        }
    }
    class Program
    {
        static Listener _listener = new Listener();
        static void Main(string[] args)
        {
            // IP 주소
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];

            // Port 번호
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Listen(endPoint, () => new GameSession());
            Console.WriteLine("Listening...");

            while (true) ;
        }
    }
}
