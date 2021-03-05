using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using core;

namespace test
{
    class Program
    {
        static Listener _listener = new Listener();
        public static GameRoom Room = new GameRoom();

        static void FlushRoom()
        {
            Room.Push(() => Room.Flush());

            // 250ms 이후에 다시 실행: 예약
            JobTimer.Instance.Push(FlushRoom, 250);
        }

        static void Main(string[] args)
        {
            // IP 주소
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];

            // Port 번호
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Listen(endPoint, () => SessionManager.Instance.Generate());
            Console.WriteLine("Listening...");

            // int roomTick = 0;
            // while (true)
            // {
            //     int now = System.Environment.TickCount;
            //     if (roomTick < now)
            //     {
            //         Room.Push(() => Room.Flush());
            //         roomTick = now + 250;
            //     }
            // }

            // FlushRoom();
            JobTimer.Instance.Push(FlushRoom, 250);

            while (true)
            {
                JobTimer.Instance.Flush();
            }
        }
    }
}
