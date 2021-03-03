using System;
using System.Collections.Generic;
using System.Text;
using core;
using test;

class PacketHandler
{
    public static void C_ChatHandler(PacketSession session, IPacket packet)
    {
        C_Chat chatPacket = packet as C_Chat;
        ClientSession clientSession = session as ClientSession;

        if (clientSession.Room == null)
            return;

        GameRoom room = clientSession.Room;
        room.Push(
            () => room.BroadCast(clientSession, chatPacket.chat)
        );
    }
}