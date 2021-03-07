using System;
using System.Collections.Generic;
using System.Text;
using core;

public enum PacketId
{
    C_Chat = 1,
	S_Chat = 2,
	
}

interface IPacket
{
    ushort Protocol { get; }
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}


class C_Chat : IPacket
{
    public string chat;

    public ushort Protocol { get { return (ushort)PacketId.C_Chat; } }

    public void Read(ArraySegment<byte> seg)
    {
        ushort count = 0;

        
        count += sizeof(ushort);
        count += sizeof(ushort);

        ushort chatLen = BitConverter.ToUInt16(seg.Array, seg.Offset + count);
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(seg.Array, seg.Offset + count, chatLen);
		count += chatLen;
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes((ushort)PacketId.C_Chat), 0, seg.Array, seg.Offset + count, sizeof(ushort));
        count += sizeof(ushort);

        ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, seg.Array, seg.Offset + count + sizeof(ushort));
		Array.Copy(BitConverter.GetBytes(chatLen), 0, seg.Array, seg.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		count += chatLen;

        Array.Copy(BitConverter.GetBytes(count), 0, seg.Array, seg.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

class S_Chat : IPacket
{
    public int playerId;
	public string chat;

    public ushort Protocol { get { return (ushort)PacketId.S_Chat; } }

    public void Read(ArraySegment<byte> seg)
    {
        ushort count = 0;

        
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(seg.Array, seg.Offset + count);
		count += sizeof(int);
		ushort chatLen = BitConverter.ToUInt16(seg.Array, seg.Offset + count);
		count += sizeof(ushort);
		this.chat = Encoding.Unicode.GetString(seg.Array, seg.Offset + count, chatLen);
		count += chatLen;
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes((ushort)PacketId.S_Chat), 0, seg.Array, seg.Offset + count, sizeof(ushort));
        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(this.playerId), 0, seg.Array, seg.Offset + count, sizeof(int));
		count += sizeof(int);
		ushort chatLen = (ushort)Encoding.Unicode.GetBytes(this.chat, 0, this.chat.Length, seg.Array, seg.Offset + count + sizeof(ushort));
		Array.Copy(BitConverter.GetBytes(chatLen), 0, seg.Array, seg.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		count += chatLen;

        Array.Copy(BitConverter.GetBytes(count), 0, seg.Array, seg.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

