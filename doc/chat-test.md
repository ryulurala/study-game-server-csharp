---
title: "Chat Test"
category: Game-Server
tags: [chat, session-manager, broadcast]
date: "2021-03-01"
---

## Chat Test

- BroadCast 이용

  > 한 명이 메시지를 보내면 모두에게 보내줘야 한다.  
  > Multi-Thread 환경은 Lock만 잡으면 안정적이만 성능면에서 매우 부하가 심하다.  
  > 실질적으로 Queue에 넣고 다른 일을 하게끔 로직을 만든다.(Job or Task)

- 순서
  1. SessionManager.cs 작성
  2. GameRoom.cs 작성
  3. PDL.xml 변경
  4. PacketHandler 작성

### Chat Test: SessionManager.cs

- Multi-Thread 환경에서 Session과 관련된 코드는 Lock 필요!

- Server's SessionManager

  > ClientSession을 관리

  - `Generate()`
    > ClientSession을 생성 후 `Dictionary<>`로 관리
  - `Find()`
    > `Dictionary<>`에서 ClientSession을 찾음
  - `Remove()`
    > `Dictionary<>`에서 ClientSession을 지움

  ```cs
  class SessionManager
  {
      // Singleton
      static SessionManager _session = new SessionManager();
      public static SessionManager Instance { get { return _session; } }

      int _sessionId = 0;
      Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();

      object _lock = new object();  // for. multi-thread

      public ClientSession Generate()
      {
          lock (_lock)
          {
              int sessionId = ++_sessionId;

              ClientSession session = new ClientSession();
              session.SessionId = sessionId;
              _sessions.Add(sessionId, session);

              Console.WriteLine($"Connected: {sessionId}");

              return session;
          }
      }

      public ClientSession Find(int id)
      {
          lock (_lock)
          {
              ClientSession session = null;
              _sessions.TryGetValue(id, out session);
              return session;
          }
      }

      public void Remove(ClientSession session)
      {
          lock (_lock)
          {
              _sessions.Remove(session.SessionId);
          }
      }
  }
  ```

- Client's SessionManager

  > ServerSession을 관리

  - `Generate()`
    > ServerSession을 생성 후 `List<>`로 관리
  - `SendForEach()`
    > 모든 ServerSession에 Send()

  ```cs
  class SessionManager
  {
      static SessionManager _session = new SessionManager();
      public static SessionManager Instance { get { return _session; } }

      List<ServerSession> _sessions = new List<ServerSession>();
      object _lock = new object();

      public ServerSession Generate()
      {
          lock (_lock)
          {
              ServerSession session = new ServerSession();
              _sessions.Add(session);
              return session;
          }
      }

      public void SendForEach()
      {
          lock (_lock)
          {
              foreach (ServerSession session in _sessions)
              {
                  C_Chat chatPacket = new C_Chat();
                  chatPacket.chat = $"Hello Server !";
                  ArraySegment<byte> segment = chatPacket.Write();

                  session.Send(segment);
              }
          }
      }
  }
  ```

### Chat Test: GameRoom.cs

- 채팅방
- `List<>`로 들어와있는 ClientSession 관리
-
- GameRoom.cs

  - BroadCast()
    > Client의 Packet을 분석  
    > Server의 Packet을 생성 후 모든 Session에게 전송  
    > 병목현상이 일어나는 부분
  - Enter()
    > `List<>`에 들어온 ClientSession 추가
  - Leave()
    > `List<>`에서 ClientSession 제거

  ```cs
  class GameRoom
  {
      List<ClientSession> _sessions = new List<ClientSession>();
      object _lock = new object();

      public void BroadCast(ClientSession session, string chat)
      {
          S_Chat packet = new S_Chat();
          packet.playerId = session.SessionId;
          packet.chat = $"{chat} I am {packet.playerId}";
          ArraySegment<byte> segment = packet.Write();

          lock (_lock)
          {
              foreach (ClientSession s in _sessions)
                  s.Send(segment);
          }
      }

      public void Enter(ClientSession session)
      {
          lock (_lock)
          {
              _sessions.Add(session);
              session.Room = this;
          }
      }

      public void Leave(ClientSession session)
      {
          lock (_lock)
          {
              _sessions.Remove(session);
          }
      }
  }
  ```

### Chat Test: PDL.xml

- C_Chat

  > Client's packet  
  > Client가 보내는 메시지(string chat)

- S_Chat

  > Server's Packet  
  > Server가 보내는 메시지(string chat)

```cs
<?xml version="1.0" encoding="utf-8" ?>
<PDL>
  <packet name="C_Chat">
    <string name="chat"/>
  </packet>
  <packet name="S_Chat">
    <int name="playerId"/>
    <string name="chat"/>
  </packet>
</PDL>
```

### Chat Test: PacketHandler.cs

- Server's Handler

  - C_ChatHandler()
    > Client's Packet(C_Chat) Handling func.  
    > BroadCast: 모든 ClientSession에게 Send()

  ```cs
  class PacketHandler
  {
      public static void C_ChatHandler(PacketSession session, IPacket packet)
      {
          C_Chat chatPacket = packet as C_Chat;
          ClientSession clientSession = session as ClientSession;
          if (clientSession.Room == null)
              return;

          clientSession.Room.BroadCast(clientSession, chatPacket.chat);
      }
  }
  ```

- Client's PacketHandler

  - S_ChatHandler()
    > Server's Packet(S_Chat) Handling func.  
    > Only. 출력

  ```cs
  class PacketHandler
  {
      public static void S_ChatHandler(PacketSession session, IPacket packet)
      {
          S_Chat chatPacket = packet as S_Chat;
          ServerSession serverSession = session as ServerSession;

          // if (chatPacket.playerId == 1)
          Console.WriteLine($"message: {chatPacket.chat}");
      }
  }
  ```

---
