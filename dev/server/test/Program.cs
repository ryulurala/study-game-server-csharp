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
        static void Main(string[] args)
        {
            // IP 주소
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];

            // Port 번호
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Listen(endPoint, () => new ClientSession());
            Console.WriteLine("Listening...");

            while (true) ;
        }
    }
}
