using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using core;

namespace dummy
{
    class ServerSession : PacketSession
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Onconnected: {endPoint}");

        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
            // Console.WriteLine($"[From Server] {recvData}");
        }

        public override void OnSend(int numOfBytes)
        {
            // Console.WriteLine($"Transferred Byte: {numOfBytes}");
        }
    }
}