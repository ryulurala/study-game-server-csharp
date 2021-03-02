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
        public int SessionId { get; set; }
        public GameRoom Room { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Onconnected: {endPoint}");

            Program.Room.Enter(this);
        }
        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }
        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);
            if (Room != null)
            {
                Room.Leave(this);
                Room = null;
            }
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }
        public override void OnSend(int numOfBytes)
        {
            // Console.WriteLine($"Transferred Byte: {numOfBytes}");
        }
    }
}