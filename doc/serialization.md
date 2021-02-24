---
title: "Buffer"
category: Game-Server
tags:
  [
    serialization,
    deserialization,
    write,
    read,
    array-copy,
    span,
    trywritebytes,
    protocol-buffers,
    flat-buffers,
  ]
date: "2021-02-23"
---

## Serialization

- Serialization: 직렬화

  > 객체 형태의 패킷을 버퍼 안에 밀어넣는 작업  
  > Instance -> byte[]  
  > byte[]를 buffer에 밀어넣음.  
  > 네트워크 송수신 가능하도록 함.

- Deserialization: 역직렬화

  > 버퍼 안에 패킷을 객체 형태로 읽는 작업  
  > byte[] -> Instance  
  > buffer의 내용을 Instance로 읽음

- Packet
  > 일종의 Class의 Instance  
  > 보통 `.json`, `.xml` 파일로 자동화하여 정의.

### primitive type

- Packet class

  ```cs
  public abstract class Packet
  {
      public ushort size;
      public ushort packetId;

      public abstract ArraySegment<byte> Write();
      public abstract void Read(ArraySegment<byte> seg);
  }

  class PlayerInfoReq : Packet
  {
      public long playerId;

      public override ArraySegment<byte> Write(){ }
      public override void Read(ArraySegment<byte> seg){ }
  }
  ```

#### Serialization: primitive type

##### `GetBytes()`, `Array.Copy()`

- 안정적, but. 성능이 조금 떨어짐.
- GetBytes(value)
- Copy(sourceArray, sourceIndex, destArray, destIndex, length)

```cs
public ArraySegment<byte> Write()
{
    // 버퍼 공간 예약
    ArraySegment<byte> seg = SendBufferHelper.Open(4096);

    // byte[] get
    // header
    byte[] size = BitConverter.GetBytes(packet.size);    // ushort: 2
    byte[] packetId = BitConverter.GetBytes(packet.packetId);    // ushort: 2

    // data
    byte[] playerId = BitConverter.GetBytes(packet.playerId);   // long: 8

    // Serialization
    ushort count = 0;   // Offset 처리

    count += sizeof(ushort);  // packetSize type
    Array.Copy(packetId, 0, seg.Array, seg.Offset + count, 2);
    count += sizeof(ushort);  // packetId type
    Array.Copy(playerId, 0, seg.Array, seg.Offset + count, 8);
    count += sizeof(long);  // playerId type

    // packcet size는 마지막에 알 수 있으므로
    Array.Copy(size, 0, seg.Array, seg.Offset, 2);

    // 실패할 경우, null 리턴
    if (success == false)
        return null;

    // 성공할 경우, 실제 공간 리턴
    return SendBufferHelper.Close(count);
}
```

##### `TryWriteBytes()`, `Span<byte>()`

- GetBytes()의 성능을 향상시킴.
- bool TryWriteBytes(destination, value)
- new Span\<byte\>(array, offset, count)

```cs
public ArraySegment<byte> Write()
{
    // 버퍼 공간 예약
    ArraySegment<byte> seg = SendBufferHelper.Open(4096);

    ushort count = 0;   // Offset 계산
    bool success = true;  // 최종적으로 성공 여부

    count += sizeof(ushort); // packetSize type
    success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset + count, seg.Count - count), packet.packetId);
    count += sizeof(ushort); // packetId type
    success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset + count, seg.Count - count), packet.playerId);
    count += sizeof(long); // playerId type

    // packcet size는 마지막에 알 수 있으므로
    success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset, seg.Count), count);

    // 실패할 경우, null 리턴
    if (success == false)
        return null;

    // 성공할 경우, 실제 공간 리턴
    return SendBufferHelper.Close(count);
}
```

##### `TryWriteBytes()`, `Slice()`

- `Span`을 잘라서 사용: 직관적임
- bool TryWriteBytes(destination, value)
- Slice(start, length)

```cs
public ArraySegment<byte> Write()
{
    // 버퍼 공간 예약
    ArraySegment<byte> seg = SendBufferHelper.Open(4096);
    Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);

    ushort count = 0;
    bool success = true;

    count += sizeof(ushort);  // packetSize type
    success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.packetId);
    count += sizeof(ushort);  // packetId type
    success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerId);
    count += sizeof(long);  // playerId type

    // packcet size는 마지막에 알 수 있으므로
    success &= BitConverter.TryWriteBytes(span, count);

    // 실패할 경우, null 리턴
    if (success == false)
        return null;

    // 성공할 경우, 실제 공간 리턴
    return SendBufferHelper.Close(count);
}
```

#### Deserialization: primitive type

- Client에서 패킷을 속여 보낸다고 생각하고 코드를 작성해야 함.
- 충분한 공간이 있는지 `ReadOnlySpan`으로 검사

- BitConverter

  - `ToInt16`: `signed short`(16bits = 2bytes)
  - `ToUInt16`: `unsigned short`(16bits = 2bytes)

  - `ToInt32`: `signed int`(32bits = 4bytes)
  - `ToUInt32`: `unsigned int`(32bits = 4bytes)

  - `ToSingle`: `float`
  - `ToDouble`: `double`

##### `Segment Offset`

- 범위 초과에 대해 위험하다.

```cs
public void Read(ArraySegment<byte> seg)
{
    ushort count = 0;

    ushort size = BitConverter.ToUInt16(seg.Array, seg.Offset);
    count += sizeof(ushort);
    ushort id = BitConverter.ToUInt16(seg.Array, seg.Offset + count);
    count += sizeof(ushort);
    long playerId = BitConverter.ToInt64(seg.Array, seg.Offset + count);
    count += sizeof(long);
}
```

##### `ReadOnlySpan`

- 범위를 잘라서 사용하여 안정적임.
- 범위가 초과할 시에 `Exception` 발생

```cs
public void Read(ArraySegment<byte> seg)
{
    ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);

    ushort count = 0;

    ushort size = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
    count += sizeof(ushort);
    ushort packetId = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
    count += sizeof(ushort);
    long playerId = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
    count += sizeof(long);
}
```

### string type, byte[] type

- string은 크기를 예상할 수 없다.
- `C++`(UTF-8) 기준에서는 `\0`로 판별하여 보냄
- `C#`(UTF-16) 기준에서는 `string length` + `string data`로 판별

  - `byte[2](ushort)` + `byte[n](data)`

- `C#`은 기본적으로 UTF-16(Unicode)를 사용

- Packet class

  ```cs
  public abstract class Packet
  {
      public ushort size;
      public ushort packetId;

      public abstract ArraySegment<byte> Write();
      public abstract void Read(ArraySegment<byte> seg);
  }

  class PlayerInfoReq : Packet
  {
      public string name;

      public override ArraySegment<byte> Write(){ }
      public override void Read(ArraySegment<byte> seg){ }
  }
  ```

#### Serialization: string type, byte[] type

##### `GetByteCount`, `Array.Copy()`

- int GetByteCount(string)
  > `length`를 리턴

```cs
public ArraySegment<byte> Write()
{
    // 버퍼 공간 예약
    ArraySegment<byte> seg = SendBufferHelper.Open(4096);
    Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);

    ushort count = 0;   // offset
    bool success = true;  // 성공 여부

    count += sizeof(ushort);  // packet size
    count += sizeof(ushort);  // packet Id

    // string length [2](ushort)
    // UTF-16(=Unicode)
    ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
    success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);
    count += sizeof(ushort);  // ushort type

    // string byte[n](data)
    Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, seg.Array, count, nameLen);
    count += nameLen;   // string byte count

    // packcet size는 마지막에 알 수 있으므로
    success &= BitConverter.TryWriteBytes(span, count);

    // 실패할 경우, null 리턴
    if (success == false)
        return null;

    // 성공할 경우, 실제 공간 리턴
    return SendBufferHelper.Close(count);
}
```

##### `GetBytes()`

- int GetBytes(source, source offset, length, destination, destination offset)
  > `length`를 리턴

```cs
public ArraySegment<byte> Write()
{
    // 버퍼 공간 예약
    ArraySegment<byte> seg = SendBufferHelper.Open(4096);
    Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);

    ushort count = 0;   // offset
    bool success = true;  // 성공 여부

    count += sizeof(ushort);  // packet size
    count += sizeof(ushort);  // packet Id

    // byte[2](ushort), byte[n](data)
    ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, seg.Array, seg.Offset + count + sizeof(ushort));
    success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);

    count += sizeof(ushort);    // ushort type
    count += nameLen;   // string byte count

    // packcet size는 마지막에 알 수 있으므로
    success &= BitConverter.TryWriteBytes(span, count);

    // 실패할 경우, null 리턴
    if (success == false)
        return null;

    // 성공할 경우, 실제 공간 리턴
    return SendBufferHelper.Close(count);
}
```

#### Deserialization: string type, byte[] type

##### `GetString()`

- string GetString(span)

```cs
public void Read(ArraySegment<byte> seg)
{
    ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);

    ushort count = 0;

    // string
    ushort nameLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
    count += sizeof(ushort);

    this.name = Encoding.Unicode.GetString(span.Slice(count, nameLen));
    count += nameLen;
}
```

### list, struct type

- list
  - string과 마찬가지로, `list count` + `list date` 로 보낸다.
- struct

  - 내부의 함수로 Serialization, Deserialization 처리

- Packet class

  ```cs
  public abstract class Packet
  {
      public ushort size;
      public ushort packetId;

      public abstract ArraySegment<byte> Write();
      public abstract void Read(ArraySegment<byte> seg);
  }

  class PlayerInfoReq : Packet
  {
      public List<SkillInfo> skills = new List<SkillInfo>();

      public struct SkillInfo
      {
          public int id;
          public short level;
          public float duration;

          public bool Write(Span<byte> span, ref short count){ }
          public void Read(ReadOnlySpan<byte> span, ref ushort count){ }
      }

      // struct 내부적으로 처리
      public bool ArraySegment<byte> Write()
      {
          bool success = true;

          success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.id);
          count += sizeof(int);
          success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.level);
          count += sizeof(short);
          success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.duration);
          count += sizeof(float);

          return true;
      }

      // struct 내부적으로 처리
      public void Read(ArraySegment<byte> seg)
      {
          this.id = BitConverter.ToInt32(span.Slice(count, span.Length - count));
          count += sizeof(int);
          this.level = BitConverter.ToInt16(span.Slice(count, span.Length - count));
          count += sizeof(short);
          this.duration = BitConverter.ToSingle(span.Slice(count, span.Length - count));
          count += sizeof(float);
      }
  }
  ```

#### Serialization: list, struct type

```cs
public ArraySegment<byte> Write()
{
    // 예약
    ArraySegment<byte> seg = SendBufferHelper.Open(4096);
    Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);

    ushort count = 0;   // offset
    bool success = true;  // 성공 여부

    count += sizeof(ushort);  // packet size
    count += sizeof(ushort);  // packet Id

    // Skill list
    // Skill list length
    success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)skills.Count);
    count += sizeof(ushort);

    // Skill list data: count는 내부적으로 처리
    foreach (SkillInfo skill in skills)
        success &= skill.Write(span, ref count);

    // packcet size는 마지막에 알 수 있으므로
    success &= BitConverter.TryWriteBytes(span, count);

    // 실패할 경우, null 리턴
    if (success == false)
        return null;

    // 성공할 경우, 실제 공간 리턴
    return SendBufferHelper.Close(count);
}
```

#### Deserialization: list, struct type

```cs
public void Read(ArraySegment<byte> seg)
{
    // 예약
    ArraySegment<byte> seg = SendBufferHelper.Open(4096);
    Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);

    ushort count = 0;   // offset
    bool success = true;  // 성공 여부

    count += sizeof(ushort);  // packet size
    count += sizeof(ushort);  // packet Id

    // 혹시 데이터 들어있는 경우 방지
    skills.Clear();

    // Skill list
    ushort skillLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
    count += sizeof(ushort);

    // Skill data
    for (int i = 0; i < skillLen; i++)
    {
        SkillInfo skill = new SkillInfo();
        skill.Read(span, ref count);
        skills.Add(skill);
    }
}
```

### Protocl buffers vs Flat buffers

- Protocal buffers
  > 중간에 Inastance를 만들어 채우고 변환하는 작업  
  > 직관적임  
  > 구조화된 데이터를 직렬화
- Flat buffers
  > 데이터를 byte[]에 직접적으로 넣는 작업  
  > 플랫폼에 상관없음  
  > 게임 개발에 많이 사용

---
