---
title: "Packet Session"
category: Game-Server
tags: [packet-session, packet, header, sealed]
date: "2021-02-21"
---

## Packet Session

### Packet

- 실질적으로 Data를 Packet으로 주고 받는다.
- Packet은 최대한 압축해서 보내야된다.

  > Serialized: 직렬화  
  > 패킷을 이어붙여서 보냄.
  > [size(2)][packetid(2)][...][size(2)][packetId(2)][...]

#### Packet Header

1. 패킷 크기
   > 얼마나 보냈는 지, 부분적으로 왔는 지 판별 가능
2. 패킷 Id
   > 어떤 Packet인 지, 유효한 지 판별

```cs
class Packet
{
    // uint(4 byte) vs ushort(2 byte)
    // 기본적으로 ushort로 충분
    public ushort size; // 패킷 크기
    public ushort packetId; // 패킷 ID
}
```

#### LoginOkPacket 예제

```cs
class LoginOkPacket: Packet
{
    // 상속 때문에 size, packetId 멤버 변수를 가지고 있음
    public int ok;
}
```

### PacketSession

```cs
public abstract class PacketSession : Session
{
    public static readonly int HeadSize = 2;

    // sealed: 더 이상 override 불가
    // PacketSession을 상속받아도 OnRecv() 구현 불가
    public sealed override int OnRecv(ArraySegment<byte> buffer)
    {
        // if. packet: [size(2)][packetId(2)][...][size(2)][packetId(2)][...]
        int processLen = 0; // 처리한 length

        // Parsing packet
        while (true)
        {
            // 최소 헤더 파싱 여부
            if (buffer.Count < HeadSize)
                break;

            // Packet이 완전체로 도착했는지 여부
            ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            if (buffer.Count < dataSize)
                break;

            // Packet 조립
            // [size(2)][packetId(2)][...] 넘겨줌.
            // buffer.Slice() API도 가능.
            // ArraySegment는 stack 영역(= struct)이므로
            OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

            processLen += dataSize;

            // 다음 buffer slice
            buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
        }

        return processLen;  // 처리한 length
    }

    // PacketSession을 상속 받는 Class는 이 인터페이스를 override
    public abstract void OnRecvPacket(ArraySegment<byte> buffer);
}
```

---
