using System;
using System.Collections.Generic;
using System.Text;
using core;
using dummy;

class PacketHandler
{
    public static void S_BroadcastEnterGameHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastEnterGame broadcastEnterGame = packet as S_BroadcastEnterGame;
        ServerSession serverSession = session as ServerSession;
    }
    public static void S_BroadcastLeaveGameHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastLeaveGame broadcastLeaveGame = packet as S_BroadcastLeaveGame;
        ServerSession serverSession = session as ServerSession;
    }
    public static void S_PlayerListHandler(PacketSession session, IPacket packet)
    {
        S_PlayerList playerList = packet as S_PlayerList;
        ServerSession serverSession = session as ServerSession;
    }

    public static void S_BroadcastMoveHandler(PacketSession session, IPacket packet)
    {
        S_BroadcastMove broadcastMoveGame = packet as S_BroadcastMove;
        ServerSession serverSession = session as ServerSession;
    }
}