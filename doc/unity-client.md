---
title: "Unity Client"
category: Game-Server
tags: [unity, packet-queue, contents, position]
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
        lock (_lock)
        {
            if (_packetQueue.Count == 0)
                return null;

            // main thread가 pop
            return _packetQueue.Dequeue();
        }
    }

    // Main-Thread가 한 번에 PopAll하여 한꺼번에 처리
    public List<IPacket> PopAll()
    {
        List<IPacket> list = new List<IPacket>();

        lock (_lock)
        {
            while (_packetQueue.Count > 0)
                list.Add(_packetQueue.Dequeue());
        }

        return list;
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

### Contents: Position 동기화

- Dummy Client에서 Position을 전달하면 Server를 통해 Unity에서 Rendering
- Game Packet 정의는 채팅과 같다.

  > 접속, 움직임 패킷 모두 모든 플레이어에게 똑같이 알린다.(= Broadcast)

#### Contents 구현 순서

1. PDL.xml 작성(= Packet 정의)
   - `S_BroadcastEnterGame`
     > 새로운 플레이어가 접속했다는 것을 모두에게 알리는 패킷
   - `C_LeaveGame`
     > Client가 Server에게 나간다고 알리는 패킷
   - `S_BroadcastLeaveGame`
     > Server가 어떤 Client가 나갔다고 모두에게 알리는 패킷
   - `S_PlayerList`
     > 새로운 플레이어에게 현재 접속해있는 Player의 List를 전달하는 패킷
   - `C_Move`
     > Client가 Server에게 해당 좌표로 움직였다고 알리는 패킷
   - `S_BroadcastMove`
     > 어떤 Client가 움직였다고 모두에게 알리는 패킷
2. Server Contents
   - `GameRoom.cs` 작성
     > Server's Contents code  
     > 역할에 따라 각각 분리하여 Script를 작성하고 ClientSession을 갖고 있으면 효율적.
   - `PacketHandler.cs` 작성
     > Leave와 Move에 대해서 Handling Script 작성  
     > 접속은 이미 Client Session에서 OnConnected()로 실행
3. Unity Client
   - `NetworkManager.cs` 작성
     > IP, Port를 Bind, Connect  
     > Packet Receive
   - `PlayerManager.cs` 작성
     > Player, MyPlayer Component를 갖고있는 GameObject를 생성
   - `Player.cs`, `MyPlayer.cs` 작성
     > Send C_Move Packet
4. Dummy Client
   - `SessionManager.cs`
     > Random Position Packet을 전송

#### `PDL.xml` 작성

1. `S_BroadcastEnterGame`

   > 새로운 플레이어가 접속했다는 것을 모두에게 알리는 패킷

   - `int playerId`
     > 어떤 Player가 접속했는지 다른 플레이어들도 캐싱 가능하도록 Server에서 전송
   - `float posX, posY, posZ`
     > 해당 Player가 접속하자마자 Server에서 Default Player 위치를 전송

2. `C_LeaveGame`
   > Client가 Server에게 나간다고 알리는 패킷
   - `int playerId`
     > Client가 자신이 나간다고 Server에게 자신의 Id 전송
3. `S_BroadcastLeaveGame`
   > Server가 어떤 Client가 나갔다고 모두에게 알리는 패킷
   - `int playerId`
     > 어떤 Player가 나갔는지 모두에게 Id를 알리고 PlayerList를 Update하라고 전송
4. `S_PlayerList`
   > 새로운 플레이어에게 현재 접속해있는 Player의 List를 전달하는 패킷
   - `List player`
     > 모든 Player가 담긴 List  
     > Server가 캐싱하고 있는 Client Session들을 담아서 전송
     - `bool isSelf`
       > PlayerList에 담긴 Player가 자신인지 유무  
       > 자신의 PlayerId를 알 수 있음  
       > 다른 방법으로는 Player 상태 정보를 먼저 보내고 CLient가 Id를 캐싱하도록 함.
     - `int playerId`
       > PlayerList에 담긴 playerId
     - `float posX, posY, posZ`
       > 모든 Player들의 위치 정보
5. `C_Move`
   > Client가 Server에게 해당 좌표로 움직였다고 알리는 패킷  
   > Id가 Client Session에 저장돼 있으므로 playerId를 전송할 필요 [X]
   - `float posX, posY, posZ`
     > 움직였다고 Server에게 좌표 전송
6. `S_BroadcastMove`

   > 어떤 Client가 움직였다고 모두에게 알리는 패킷

   - `int playerId`
     > 어떤 Player가 움직였는지 모두에게 Id 전송
   - `float posX, posY, posZ`
     > 어떤 Player가 어떤 좌표로 움직였는지 모두에게 위치 좌표 전송

- PDL.xml

  ```xml
  <?xml version="1.0" encoding="utf-8" ?>

  <PDL>
    <packet name="S_BroadcastEnterGame">    // 1)
      <int name="playerId"/>
      <float name="posX"/>
      <float name="posY"/>
      <float name="posZ"/>
    </packet>
    <packet name="C_LeaveGame">   // 2)
      <int name="playerId"/>
    </packet>
    <packet name="S_BroadcastLeaveGame">  // 3)
      <int name="playerId"/>
    </packet>
    <packet name="S_PlayerList">    // 4)
      <list name="player">
        <bool name="isSelf"/>
        <int name="playerId"/>
        <float name="posX"/>
        <float name="posY"/>
        <float name="posZ"/>
      </list>
    </packet>
    <packet name="C_Move">    // 5)
      <float name="posX"/>
      <float name="posY"/>
      <float name="posZ"/>
    </packet>
    <packet name="S_BroadcastMove"> // 6)
      <int name="playerId"/>
      <float name="posX"/>
      <float name="posY"/>
      <float name="posZ"/>
    </packet>
  </PDL>
  ```

#### Server Contents 구현

- Server's Contents code

  - 역할에 따라 각각 분리하여 Script를 작성하고 ClientSession을 갖고 있으면 효율적이다.

- GameRoom.cs

  - `Enter()`
    1. Player를 추가(= Client Session 캐싱)
    2. 처음 접속한 Player에게 모든 Player List 전송
    3. 모든 Player에게 처음 접속한 Player 정보를 전송
  - `Leave()`
    1. 나간다는 Player 제거
    2. 모두에게 해당 Player 나갔다고 알림
  - `Move()`
    1. 받은 패킷으로 갖고있는 Session의 좌표 변경
    2. 모두에게 좌표 변경하라고 알림

  ```cs
  class GameRoom : IJobQueue
  {
      List<ClientSession> _sessions = new List<ClientSession>();
      JobQueue _jobQueue = new JobQueue();
      // 보낼 메시지 임시 저장
      List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

      public void Push(Action job)
      {
          // JobQueue에 밀어 넣기
          _jobQueue.Push(job);
      }

      public void Flush()
      {
          // 팬딩 목록을 보내는 작업
          foreach (ClientSession s in _sessions)
              s.Send(_pendingList);

          Console.WriteLine($"Flushed {_pendingList.Count} items");
          _pendingList.Clear();
      }

      // Segment를 받아서 모두에게 뿌림
      public void Broadcast(ArraySegment<byte> segment)
      {
          // Reserve Send
          _pendingList.Add(segment);
      }

      public void Enter(ClientSession session)
      {
          // 플레이어 추가
          _sessions.Add(session);
          session.Room = this;

          // 신입한테 모든 플레이어 목록 전송
          S_PlayerList players = new S_PlayerList();
          foreach (ClientSession s in _sessions)
          {
              players.players.Add(new S_PlayerList.Player()
              {
                  isSelf = (s == session),
                  playerId = s.SessionId,
                  posX = s.PosX,
                  posY = s.PosY,
                  posZ = s.PosZ,
              });
          }
          session.Send(players.Write());  // 해당 Player에게 전송

          // 신입 입장을 모두에게 알림
          S_BroadcastEnterGame enter = new S_BroadcastEnterGame();
          enter.playerId = session.SessionId;
          enter.posX = 0;
          enter.posY = 0;
          enter.posZ = 0;
          Broadcast(enter.Write());
      }

      public void Leave(ClientSession session)
      {
          // 플레이어 제거
          _sessions.Remove(session);

          // 모두에게 알림
          S_BroadcastLeaveGame leave = new S_BroadcastLeaveGame();
          leave.playerId = session.SessionId;
          Broadcast(leave.Write());
      }

      public void Move(ClientSession session, C_Move packet)
      {
          // 좌표 변경
          session.PosX = packet.posX;
          session.PosY = packet.posY;
          session.PosZ = packet.posZ;

          // 모두에게 알림
          S_BroadcastMove move = new S_BroadcastMove();
          move.playerId = session.SessionId;
          move.posX = session.PosX;
          move.posY = session.PosY;
          move.posZ = session.PosZ;
          Broadcast(move.Write());
      }
  }
  ```

- PacketHandler.cs

  > Client's Packet Handling

  - 다른 Player의 Leave, Move Handling

  ```cs
  class PacketHandler
  {
      public static void C_LeaveGameHandler(PacketSession session, IPacket packet)
      {
          ClientSession clientSession = session as ClientSession;

          if (clientSession.Room == null)
              return;

          GameRoom room = clientSession.Room;
          // Leave Handling
          room.Push(() => room.Leave(clientSession));
      }

      public static void C_MoveHandler(PacketSession session, IPacket packet)
      {
          C_Move movePacket = packet as C_Move;
          ClientSession clientSession = session as ClientSession;

          if (clientSession.Room == null)
              return;

          GameRoom room = clientSession.Room;
          // Move Handling
          room.Push(() => room.Move(clientSession, movePacket));
      }
  }
  ```

#### Unity Client 구현

- NetworkManager.cs 작성

  > `IP`, `Port`를 `Bind`, `Connect`  
  > Packet `Receive`

  ```cs
  public class NetworkManager : MonoBehaviour
  {
      // Unity's Session
      ServerSession _session = new ServerSession();
      public void Send(ArraySegment<byte> sendBuff)
      {
          _session.Send(sendBuff);
      }

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
      }

      void Update()
      {
          // 1 tick에 PacketQueue에 있는 모든 Packet을 처리
          List<IPacket> list = PacketQueue.Instance.PopAll();
          foreach (IPacket pkt in list)
              PacketManager.Instance.HandlePacket(_session, pkt);
      }
  }
  ```

- PlayerManager.cs 작성

  > Player, MyPlayer Component를 갖고있는 GameObject를 생성  
  > 수신 받은 Player에 관한 Packet을 다룸.

  ```cs
  public class PlayerManager
  {
      // Singleton
      public static PlayerManager Instance { get; } = new PlayerManager();
      MyPlayer _myPlayer;
      Dictionary<int, Player> _players = new Dictionary<int, Player>();

      // 처음에 내가 들어와서 Server에게 받은 PlayerList Packet으로 _players 갱신
      public void Add(S_PlayerList packet)
      {
          // Resource load
          Object obj = Resources.Load("Player");

          foreach (S_PlayerList.Player p in packet.players)
          {
              // PlayerList Packet을 받고 Update
              // Instantiate
              GameObject go = Object.Instantiate(obj) as GameObject;

              if (p.isSelf)
              {
                  // for. 자신
                  MyPlayer myPlayer = go.AddComponent<MyPlayer>();
                  myPlayer.PlayerId = p.playerId;
                  myPlayer.transform.position = new Vector3(p.posX, p.posY, p.posZ);

                  // 내 _myPlayer로 참조
                  _myPlayer = myPlayer;
              }
              else
              {
                  // 그 외 다른 Players
                  Player player = go.AddComponent<Player>();
                  player.PlayerId = p.playerId;
                  player.transform.position = new Vector3(p.posX, p.posY, p.posZ);

                  // _players에 추가
                  _players.Add(p.playerId, player);
              }
          }
      }

      public void Move(S_BroadcastMove pkt)
      {
          if (_myPlayer.PlayerId == pkt.playerId)
          {
              // 내가 움직일 때
              _myPlayer.transform.position = new Vector3(pkt.posX, pkt.posY, pkt.posZ);
          }
          else
          {
              // 그 외 다른 Players가 움직일 때
              Player player = null;
              if (_players.TryGetValue(pkt.playerId, out player))
              {
                  player.transform.position = new Vector3(pkt.posX, pkt.posY, pkt.posZ);
              }
          }
      }

      // 다른 Player가 들어왔다는 Packet
      public void EnterGame(S_BroadcastEnterGame pkt)
      {
          // 처음 자신에게 Broadcast에 대해서 Skip
          if (_myPlayer.PlayerId == pkt.playerId)
              return;

          // Resources load, Instantiate
          Object obj = Resources.Load("Player");
          GameObject go = Object.Instantiate(obj) as GameObject;

          // Player 추가
          Player player = go.AddComponent<Player>();
          player.transform.position = new Vector3(pkt.posX, pkt.posY, pkt.posZ);
          _players.Add(pkt.playerId, player);
      }

      public void LeaveGame(S_BroadcastLeaveGame pkt)
      {
          if (_myPlayer.PlayerId == pkt.playerId)
          {
              // 본인 Object 삭제
              GameObject.Destroy(_myPlayer.gameObject);
              _myPlayer = null;
          }
          else
          {
              // 다른 Player 삭제
              Player player = null;
              if (_players.TryGetValue(pkt.playerId, out player))
              {
                  GameObject.Destroy(player.gameObject);
                  _players.Remove(pkt.playerId);
              }
          }
      }
  }
  ```

- Player.cs, MyPlayer.cs 작성

  > Send C_Move Packet

  ```cs
  public class Player : MonoBehaviour
  {
      public int PlayerId { get; set; }
  }
  ```

  ```cs
  public class MyPlayer : Player
  {
      NetworkManager _networkManager;

      void Start()
      {
          StartCoroutine(CoSendPacket());
          _networkManager = GameObject.Find("NetworkManager").GetComponent<NetworkManager>();
      }

      IEnumerator CoSendPacket()
      {
          while (true)
          {
              yield return new WaitForSeconds(3f);

              // 3초마다 C_Move Packet 전송
              C_Move movePacket = new C_Move();
              movePacket.posX = UnityEngine.Random.Range(-50, 50);
              movePacket.posY = 1f;
              movePacket.posZ = UnityEngine.Random.Range(-50, 50);

              _networkManager.Send(movePacket.Write());
          }
      }
  }
  ```

#### Dummy Client 구현

- SessionManager.cs

  > Random Position Packet을 전송

  ```cs
  Random _rand = new Random();

  public void SendForEach()
  {
      lock (_lock)
      {
          foreach (ServerSession session in _sessions)
          {
              // Dummy로 Move Packet(= Random position) 전송
              C_Move movePacket = new C_Move();
              movePacket.posX = _rand.Next(-50, 50);
              movePacket.posY = 1;
              movePacket.posZ = _rand.Next(-50, 50);

              session.Send(movePacket.Write());
          }
      }
  }
  ```

---
