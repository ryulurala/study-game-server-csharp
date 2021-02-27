using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using core;

namespace test
{
    class ClientSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Onconnected: {endPoint}");

            Thread.Sleep(3000);

            Disconnect();
        }
        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }
        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }
        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred Byte: {numOfBytes}");
        }
    }
}