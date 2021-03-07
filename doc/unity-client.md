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

- Unity의 Main-Thread외에 다른 Thread가 Unity의 Component로 실행 불가 정책이 있다.
  > Sub-Thread가 Unity의 GameObject를 건들거나 Monobehaviour를 건들 수 없다.
- Packet Queue를 이용한 Multi-thread 환경 구축

  - Push()
    > Background thread가 사용  
    > Only. Packet을 Deserailizing하고 밀어넣는 작업
  - Pop()
    > Unity Main-thread가 사용  
    > Queue에서 빼내어 값으로 사용

- Unity의 Main thread

  1. Packet Queue에서 Pop
  2. Packet Handling

- Background thread
  1. Packet을 조립(Deserialize)
  2. Packet Queue에 Push

#### `PacketQueue.cs`

```cs
// Monobehaviour 상속 X
public class PacketQueue
{
    // Singleton으로 사용
    public static PacketQueue Instance { get; } = new PacketQueue();
    // Packet을 담고있는 Queue
    Queue<IPacket> _packetQueue = new Queue<IPacket>();
    // for. multi-thread
    object _lock = new object();

    // Sub-Thread가 밀어 넣기
    public void Push(IPacket packet)
    {
        lock (_lock)
        {
            // background thread가 push
            _packetQueue.Enqueue(packet);
        }
    }

    // Main-Thread가 Pop 해서 사용
    public IPacket Pop()
    {
        // lock??
        lock (_lock)
        {
            if (_packetQueue.Count == 0)
                return null;

            // main thread가 pop
            return _packetQueue.Dequeue();
        }
    }
}
```

#### `ClientPacketManager.cs` 수정

- Unity-Client와 Dummy-Client를 구분하여 Handling
  - Dummy-Client는 HandlePacket()을 바로 실행
  - Unity-Client는 Main-Thread(= Update())에서 Packet Queue에서 Pop -> HandlePacket()을 실행

```cs
class PacketManager
{
    #region Singleton
    static PacketManager _instance = new PacketManager();
    public static PacketManager Instance{ get { return _instance; } }
    #endregion

    // 생성할 때 Register() 자동으로 하도록
    PacketManager()
    {
        Register();
    }

    // return IPacket; 하는 Action
    Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>> _makeFunc = new Dictionary<ushort, Func<PacketSession, ArraySegment<byte>, IPacket>>();
    // Handling call-back func를 담는 Action
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {
        _makeFunc.Add((ushort)PacketId.S_Chat, MakePacket<S_Chat>);
        _handler.Add((ushort)PacketId.S_Chat, PacketHandler.S_ChatHandler);
    }

    // Parameter로 Option을 받음(= onRecvCallback)
    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer, Action<PacketSession, IPacket> onRecvCallback = null)
    {
        ushort count = 0;

        // PacketId, PacketSize Parsing
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Func<PacketSession, ArraySegment<byte>, IPacket> func = null;

        // 조립할 수 있는 Packet인지
        if (_makeFunc.TryGetValue(id, out func))
        {
            // Handling
            IPacket packet = func.Invoke(session, buffer);
            // Option(= onRecvCallback) 유무에 따라서
            if (onRecvCallback == null)
                HandlePacket(session, packet);  // Dummy-Clinet Handling
            else
                onRecvCallback.Invoke(session, packet); // Unity-Client Handling
        }

    }

    T MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        T pkt = new T();
        pkt.Read(buffer);   // Deserialize

        return pkt;
    }

    public void HandlePacket(PacketSession session, IPacket packet)
    {
        // Packet Handling
        // Unity-Client는 Update()에서 Handling
        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(packet.Protocol, out action))
            action.Invoke(session, packet);
    }
}

```

#### `ServerSession.cs` 수정

- Call-back function으로 Packet Queue에 Push()

```cs
public override void OnRecvPacket(ArraySegment<byte> buffer)
{
    // 옵션으로 Packet Queue에 Push
    // Handling은 Unity의 Main-Thread에서 진행
    PacketManager.Instance.OnRecvPacket(this, buffer, (s, p) => PacketQueue.Instance.Push(p));
}
```

#### `NetworkManager.cs`

- Packet Handling 실행
- Send Packet
  > Coroutine으로 일정 주기로 Send

```cs
public class NetworkManager : MonoBehaviour
{
    // Unity는 Session 1개(= Only. Main-thread)
    ServerSession _session = new ServerSession();

    void Start()
    {
        // IP 주소
        string host = Dns.GetHostName();
        IPHostEntry ipHost = Dns.GetHostEntry(host);
        IPAddress ipAddr = ipHost.AddressList[0];

        // Port 번호
        IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

        Connector connector = new Connector();

        connector.Connect(endPoint, () => _session);

        StartCoroutine(CoSendPacket());
    }

    void Update()
    {
        // Packet Queue에서 Pop --> Packet Handling
        IPacket packet = PacketQueue.Instance.Pop();
        if (packet != null)
            PacketManager.Instance.HandlePacket(_session, packet);
    }

    IEnumerator CoSendPacket()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);
            // 3초마다 실행

            C_Chat chatPacket = new C_Chat();
            chatPacket.chat = "Hello Unity !";
            ArraySegment<byte> segment = chatPacket.Write();

            // SendQueue에 넣기
            // Background-thread가 Send
            _session.Send(segment);
        }
    }
}
```

---
