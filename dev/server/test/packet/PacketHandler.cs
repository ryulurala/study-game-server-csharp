using System;
using System.Collections.Generic;
using System.Text;
using core;

class PacketHandler
{
    public static void C_PlayerInfoReqHandler(PacketSession session, IPacket packet)
    {
        C_PlayerInfoReq pkt = new C_PlayerInfoReq();
        Console.WriteLine($"PlayerInfoReq: {pkt.playerId}");

        foreach (C_PlayerInfoReq.Skill skill in pkt.skills)
        {
            Console.WriteLine($"Skill: {skill.id}, {skill.level}, {skill.duration}");
        }
    }
}