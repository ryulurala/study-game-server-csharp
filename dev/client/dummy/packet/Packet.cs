using System;
using System.Collections.Generic;
using System.Text;
using core;

public enum PacketId
{
    S_BroadcastEnterGame = 1,
	C_LeaveGame = 2,
	S_BroadcastLeaveGame = 3,
	S_PlayerList = 4,
	C_Move = 5,
	S_BroadcastMove = 6,
	
}

public interface IPacket
{
    ushort Protocol { get; }
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}


public class S_BroadcastEnterGame : IPacket
{
    public int playerId;
	public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol { get { return (ushort)PacketId.S_BroadcastEnterGame; } }

    public void Read(ArraySegment<byte> seg)
    {
        ushort count = 0;
        
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(seg.Array, seg.Offset + count);
		count += sizeof(int);
		this.posX = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
		this.posY = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
		this.posZ = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes((ushort)PacketId.S_BroadcastEnterGame), 0, seg.Array, seg.Offset + count, sizeof(ushort));
        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(this.playerId), 0, seg.Array, seg.Offset + count, sizeof(int));
		count += sizeof(int);
		Array.Copy(BitConverter.GetBytes(this.posX), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posY), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posZ), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);

        Array.Copy(BitConverter.GetBytes(count), 0, seg.Array, seg.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

public class C_LeaveGame : IPacket
{
    public int playerId;

    public ushort Protocol { get { return (ushort)PacketId.C_LeaveGame; } }

    public void Read(ArraySegment<byte> seg)
    {
        ushort count = 0;
        
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(seg.Array, seg.Offset + count);
		count += sizeof(int);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes((ushort)PacketId.C_LeaveGame), 0, seg.Array, seg.Offset + count, sizeof(ushort));
        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(this.playerId), 0, seg.Array, seg.Offset + count, sizeof(int));
		count += sizeof(int);

        Array.Copy(BitConverter.GetBytes(count), 0, seg.Array, seg.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

public class S_BroadcastLeaveGame : IPacket
{
    public int playerId;

    public ushort Protocol { get { return (ushort)PacketId.S_BroadcastLeaveGame; } }

    public void Read(ArraySegment<byte> seg)
    {
        ushort count = 0;
        
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(seg.Array, seg.Offset + count);
		count += sizeof(int);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes((ushort)PacketId.S_BroadcastLeaveGame), 0, seg.Array, seg.Offset + count, sizeof(ushort));
        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(this.playerId), 0, seg.Array, seg.Offset + count, sizeof(int));
		count += sizeof(int);

        Array.Copy(BitConverter.GetBytes(count), 0, seg.Array, seg.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

public class S_PlayerList : IPacket
{
    
	public class Player
	{
	    public bool isSelf;
		public int playerId;
		public float posX;
		public float posY;
		public float posZ;
	
	    public void Read(ArraySegment<byte> seg, ref ushort count)
	    {
	        this.isSelf = BitConverter.ToBoolean(seg.Array, seg.Offset + count);
			count += sizeof(bool);
			this.playerId = BitConverter.ToInt32(seg.Array, seg.Offset + count);
			count += sizeof(int);
			this.posX = BitConverter.ToSingle(seg.Array, seg.Offset + count);
			count += sizeof(float);
			this.posY = BitConverter.ToSingle(seg.Array, seg.Offset + count);
			count += sizeof(float);
			this.posZ = BitConverter.ToSingle(seg.Array, seg.Offset + count);
			count += sizeof(float);
	    }
	
	    public bool Write(ArraySegment<byte> seg, ref ushort count)
	    {
	        Array.Copy(BitConverter.GetBytes(this.isSelf), 0, seg.Array, seg.Offset + count, sizeof(bool));
			count += sizeof(bool);
			Array.Copy(BitConverter.GetBytes(this.playerId), 0, seg.Array, seg.Offset + count, sizeof(int));
			count += sizeof(int);
			Array.Copy(BitConverter.GetBytes(this.posX), 0, seg.Array, seg.Offset + count, sizeof(float));
			count += sizeof(float);
			Array.Copy(BitConverter.GetBytes(this.posY), 0, seg.Array, seg.Offset + count, sizeof(float));
			count += sizeof(float);
			Array.Copy(BitConverter.GetBytes(this.posZ), 0, seg.Array, seg.Offset + count, sizeof(float));
			count += sizeof(float);
	
	        return true;
	    }
	}
	public List<Player> players = new List<Player>();
	

    public ushort Protocol { get { return (ushort)PacketId.S_PlayerList; } }

    public void Read(ArraySegment<byte> seg)
    {
        ushort count = 0;
        
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.players.Clear();
		ushort playerLen = BitConverter.ToUInt16(seg.Array, seg.Offset + count);
		count += sizeof(ushort);
		for (int i = 0; i < playerLen; i++)
		{
		    Player player = new Player();
		    player.Read(seg, ref count);
		    players.Add(player);
		}
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes((ushort)PacketId.S_PlayerList), 0, seg.Array, seg.Offset + count, sizeof(ushort));
        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(this.players.Count), 0, seg.Array, seg.Offset + count, sizeof(ushort));
		count += sizeof(ushort);
		foreach (Player player in this.players)
		    player.Write(seg, ref count);

        Array.Copy(BitConverter.GetBytes(count), 0, seg.Array, seg.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

public class C_Move : IPacket
{
    public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol { get { return (ushort)PacketId.C_Move; } }

    public void Read(ArraySegment<byte> seg)
    {
        ushort count = 0;
        
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.posX = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
		this.posY = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
		this.posZ = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes((ushort)PacketId.C_Move), 0, seg.Array, seg.Offset + count, sizeof(ushort));
        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(this.posX), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posY), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posZ), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);

        Array.Copy(BitConverter.GetBytes(count), 0, seg.Array, seg.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

public class S_BroadcastMove : IPacket
{
    public int playerId;
	public float posX;
	public float posY;
	public float posZ;

    public ushort Protocol { get { return (ushort)PacketId.S_BroadcastMove; } }

    public void Read(ArraySegment<byte> seg)
    {
        ushort count = 0;
        
        count += sizeof(ushort);
        count += sizeof(ushort);

        this.playerId = BitConverter.ToInt32(seg.Array, seg.Offset + count);
		count += sizeof(int);
		this.posX = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
		this.posY = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
		this.posZ = BitConverter.ToSingle(seg.Array, seg.Offset + count);
		count += sizeof(float);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;

        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes((ushort)PacketId.S_BroadcastMove), 0, seg.Array, seg.Offset + count, sizeof(ushort));
        count += sizeof(ushort);

        Array.Copy(BitConverter.GetBytes(this.playerId), 0, seg.Array, seg.Offset + count, sizeof(int));
		count += sizeof(int);
		Array.Copy(BitConverter.GetBytes(this.posX), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posY), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);
		Array.Copy(BitConverter.GetBytes(this.posZ), 0, seg.Array, seg.Offset + count, sizeof(float));
		count += sizeof(float);

        Array.Copy(BitConverter.GetBytes(count), 0, seg.Array, seg.Offset, sizeof(ushort));

        return SendBufferHelper.Close(count);
    }
}

