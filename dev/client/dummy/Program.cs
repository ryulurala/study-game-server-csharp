using System;
using System.Net;
using System.Text;
using System.Threading;
using core;

namespace dummy
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

            Connector connector = new Connector();

            connector.Connect(endPoint, () => new ServerSession());

            while (true) ;
        }
    }
}
