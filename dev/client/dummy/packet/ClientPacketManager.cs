using System;
using System.Collections.Generic;
using core;

class PacketManager
{
    #region Singleton
    // 오직 하나의 인스턴스
    static PacketManager _instance;
    public static PacketManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PacketManager();
            return _instance;
        }
    }
    #endregion

    // <Protocol Id, Receive Handler>
    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    // <Protocol Id, Packet Handler> 
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {
        // MakePacket() 등록
        _onRecv.Add((ushort)PacketId.S_Test, MakePacket<S_Test>);
        // Packet Handler 등록
        _handler.Add((ushort)PacketId.S_Test, PacketHandler.S_TestHandler);
    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort count = 0;

        // Deserializing Packet Id, Packet Size
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        // Packet 조립 콜백 실행
        Action<PacketSession, ArraySegment<byte>> action = null;
        if (_onRecv.TryGetValue(id, out action))
            action.Invoke(session, buffer);
    }

    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        // Packet 생성
        T pkt = new T();

        // Deserializing Packet data
        pkt.Read(buffer);

        // Packet Handler 콜백 실행
        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(pkt.Protocol, out action))
            action.Invoke(session, pkt);
    }
}
