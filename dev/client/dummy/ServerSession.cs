using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using core;

namespace dummy
{
    class ServerSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"Onconnected: {endPoint}");

            C_PlayerInfoReq packet = new C_PlayerInfoReq() { playerId = 1001, name = "ABCD" };
            packet.skills.Add(new C_PlayerInfoReq.Skill() { id = 101, level = 1, duration = 3.0f });
            packet.skills.Add(new C_PlayerInfoReq.Skill() { id = 202, level = 2, duration = 4.0f });
            packet.skills.Add(new C_PlayerInfoReq.Skill() { id = 303, level = 3, duration = 5.0f });
            packet.skills.Add(new C_PlayerInfoReq.Skill() { id = 404, level = 4, duration = 6.0f });
            // for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> seg = packet.Write();

                if (seg != null)
                    Send(seg);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Server] {recvData}");

            return buffer.Count;
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred Byte: {numOfBytes}");
        }
    }
}