---
title: "Unity Client"
category: Game-Server
tags: [unity, packet-queue]
date: "2021-03-08"
---

## Unity Client

### Unity `C#` 제약

- Unity는 Single-Thread 기반이므로 Main-Thread외에 다른 Thread Component 사용 불가
  - Unity Component를 건들지 않는 선에서 Background에서 Thread를 실행하도록 함.
- `Span<>`, `TryWriteBytes()` 사용 불가
  - `Array.Copy(byte[], ...)`로 구성
- 구조체를 null로 반환 불가
  - `return default(type)`로 구성
- 공통된 Code는 DLL로 사용
  - `Assets`-`Plugins` 폴더에 `DLL`(Class Library) 추가

#### `PacketFormat.cs` 수정

- `Span<>`, `ReadOnlySpan<>` 사용 불가
  - `ArraySegment<>.Offset` 이용
- `TryWriteBytes() + Span<>.Slice()` 사용 불가
  - `GetBytes()` + `ArraySegment<>.Offset` 이용

##### string packetFormat

- `ReadOnlySpan<byte>`: [X]
- `TryWriteBytes()` --> `Array.Copy()`

```cs
public void Read(ArraySegment<byte> seg)
{{
    ushort count = 0;

    // ReadOnlySpan 사용 불가
    ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);
    count += sizeof(ushort);
    count += sizeof(ushort);

    {2} // 멤버 변수 READ
}}

public ArraySegment<byte> Write()
{{
    ArraySegment<byte> seg = SendBufferHelper.Open(4096);
    ushort count = 0;

    // TryWriteBytes() 사용 불가
    bool success = true;

    // Span<> 사용 불가
    Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);

    count += sizeof(ushort);
    // TryWriteBytes() 사용 불가
    success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketId.{0});  // 패킷 이름
    count += sizeof(ushort);

    {3} // 멤버 변수 Write

    success &= BitConverter.TryWriteBytes(span, count); // TryWriteBytes() 사용 불가

    // TryWriteBytes() 사용 불가
    if (success == false)
        return null;

    return SendBufferHelper.Close(count);
}}
```

```cs
public void Read(ArraySegment<byte> seg)
{{
    ushort count = 0;

    count += sizeof(ushort);
    count += sizeof(ushort);

    {2} // 멤버 변수 READ
}}

public ArraySegment<byte> Write()
{{
    ArraySegment<byte> seg = SendBufferHelper.Open(4096);
    ushort count = 0;

    count += sizeof(ushort);

    // TryWriteBytes() -> Copy()
    Array.Copy(BitConverter.GetBytes((ushort)PacketId.{0}), 0, seg.Array, seg.Offset + count, sizeof(ushort));  // 패킷 이름
    count += sizeof(ushort);

    {3} // 멤버 변수 Write

    // TryWriteBytes() -> Copy(), 마지막은 Offset 처음부터(for. size)
    Array.Copy(BitConverter.GetBytes(count), 0, seg.Array, seg.Offset, sizeof(ushort));

    return SendBufferHelper.Close(count);
}}
```

##### string memberListFormat

- `ReadOnlySpan<byte>` --> `ArraySegment<byte>`

```cs
// Parameter 변경
public void Read(ReadOnlySpan<byte> span, ref ushort count)
{{
    {3} // 멤버 변수 Read
}}
```

```cs
// Parameter 변경
public void Read(ArraySegment<byte> seg, ref ushort count)
{{
    {3} // 멤버 변수 Read
}}
```

##### string readFormat, writeFormat

- `Span<byte>.Slice()` --> `ArraySegment<byte>.Offset`
- `TryWriteBytes()`, `Span<>` --> `GetBytes()`, `ArraySegment<>`

```cs
// {0} 변수 이름
// {1} To- 변수 형식
// {2} 변수 형식
public static string readFormat =
@"this.{0} = BitConverter.{1}(span.Slice(count, span.Length - count));
count += sizeof({2});";

// Span<byte>.Slice() --> ArraySegment<byte>.Offset
public static string readFormat =
@"this.{0} = BitConverter.{1}(seg.Array, seg.Offset + count);
count += sizeof({2});";
```

```cs
// {0} 변수 이름
// {1} 변수 형식
public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.{0});
count += sizeof({1});";

// TryWriteBytes(), Span<byte>.Slice() --> GetBytes(), ArraySegment<byte>.Offset
public static string writeFormat =
@"Array.Copy(BitConverter.GetBytes(this.{0}), 0, seg.Array, seg.Offset + count, sizeof({1}));
count += sizeof({1});";
```

##### string readStringFormat, writeStringFormat

- `Span<byte>.Slice()` --> `ArraySegment<byte>.Offset`
- `TryWriteBytes()`, `Span<>` --> `GetBytes()`, `ArraySegment<>`

```cs
// {0} 변수 이름
public static string readStringFormat =
@"ushort {0}Len = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(span.Slice(count, {0}Len));
count += {0}Len;";

// Span<byte>.Slice() --> ArraySegment<byte>.Offset
public static string readStringFormat =
@"ushort {0}Len = BitConverter.ToUInt16(seg.Array, seg.Offset + count);
count += sizeof(ushort);
this.{0} = Encoding.Unicode.GetString(seg.Array, seg.Offset + count, {0}Len);
count += {0}Len;";
```

```cs
// {0} 변수 이름
public static string writeStringFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, seg.Array, seg.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), {0}Len);
count += sizeof(ushort);
count += {0}Len;";


// TryWriteBytes(), Span<byte>.Slice() --> GetBytes(), ArraySegment<byte>.Offset
public static string writeStringFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, seg.Array, seg.Offset + count + sizeof(ushort));
Array.Copy(BitConverter.GetBytes({0}Len), 0, seg.Array, seg.Offset + count, sizeof(ushort));
count += sizeof(ushort);
count += {0}Len;";
```

##### string readListFormat

- `Span<byte>.Slice()` --> `ArraySegment<byte>.Offset`
- `TryWriteBytes()`, `Span<>` --> `GetBytes()`, `ArraySegment<>`

```cs
// {0} 리스트 이름 [대문자]
// {1} 리스트 이름 [소문자]
public static string readListFormat =
@"this.{1}s.Clear();
ushort {1}Len = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
count += sizeof(ushort);
for (int i = 0; i < {1}Len; i++)
{{
    {0} {1} = new {0}();
    {1}.Read(span, ref count);
    {1}s.Add({1});
}}";

// Span<byte>.Slice() --> ArraySegment<byte>.Offset
public static string readListFormat =
@"this.{1}s.Clear();
ushort {1}Len = BitConverter.ToUInt16(seg.Array, seg.Offset + count);
count += sizeof(ushort);
for (int i = 0; i < {1}Len; i++)
{{
    {0} {1} = new {0}();
    {1}.Read(span, ref count);
    {1}s.Add({1});
}}";
```

```cs
// {0} 리스트 이름 [대문자]
// {1} 리스트 이름 [소문자]
public static string writeListFormat =
@"success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)this.{1}s.Count);
count += sizeof(ushort);
foreach ({0} {1} in this.{1}s)
    success &= {1}.Write(span, ref count);";

// TryWriteBytes(), Span<byte>.Slice() --> GetBytes(), ArraySegment<byte>.Offset
public static string writeListFormat =
@"Array.Copy(BitConverter.GetBytes(this.{1}s.Count), 0, seg.Array, seg.Offset + count, sizeof(ushort));
count += sizeof(ushort);
foreach ({0} {1} in this.{1}s)
    {1}.Write(seg, ref count);";
```

### Packet Queue

- Unity의 Main-Thread외에 다른 Thread가 Unity의 Component로 실행 불가한 정책
- Unity에서 Multi-thread 환경은 가능
  - Unity Component(Main thread) + Packet Queue(Background thread) 로 구성

---
