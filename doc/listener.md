---
title: "Listener"
category: Game-Server
tags: [c#, socket, server, listener, async-listener]
date: "2021-02-18"
---

## Listener

### Project Library 연결

- `core.csproj`

  > `Library` 로 사용할 Project  
  > `OutputType`을 `Library`로 수정한다.

  ```xml
  <PropertyGroup>
    <OutputType>Library</OutputType>
    <TargetFramework>netcoreapp3.1</TargetFramework>
  </PropertyGroup>
  ```

- `test.csproj`

  > 실제 실행할 `exe` Project  
  > `ProjectReference`(`Library`) 를 `Include` 해준다.

  ```xml
  <ItemGroup>
    <ProjectReference Include="../core/core.csproj"/>
  </ItemGroup>
  ```

### AsyncListener

- Process

  1. init()
     - `new Socket()`
     - `bind()`
     - `Accept()` 결과 콜백 이벤트 등록
     - `listen()`
  2. Accept() 예약
     - `AcceptAsync()`를 이용한 `Pending` 여부
  3. Accept() 완료
     - `init()`으로 등록한 콜백 이벤트 `Invoke`
     - `2.` 다시 실행

- Code

  ```cs
  public class Listener
  {
      Socket _listenSocket;   // 서버의 소켓
      Action<Socket> _onAcceptHandler;  // Event Action

      public void init(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
      {
          // 서버 소켓을 new, bind, event 연결
          _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

          // Accept 완료 후 실행될 콜백 Event 연결
          _onAcceptHandler += onAcceptHandler;

          // Bind(), Listen()
          _listenSocket.Bind(endPoint);
          _listenSocket.Listen(10);

          // 이벤트 방식으로 비동기 구현(콜백으로 전달)
          SocketAsyncEventArgs args = new SocketAsyncEventArgs();
          args.Completed += new EventHandler<SocketAsyncEventArgs>  (OnAcceptCompleted);  // 연결됐을 때, 실행할 이벤트 핸들러 등록
          RegisterAccept(args);   // Accept() 예약

          void RegisterAccept(SocketAsyncEventArgs args)
          {
              // 기존에 있던 클라이언트 소켓 clear
              args.AcceptSocket = null;

              // Accept()를 요청/등록
              bool pending = _listenSocket.AcceptAsync(args);

              if (!pending)
              {
                  //보류 없이 접속됨.
                  OnAcceptCompleted(null, args);
              }

              // 접속 보류됨.
              // 서버가 return 시킴.
          }

          void OnAcceptCompleted(object sender, SocketAsyncEventArgs argss)
          {
              // 콜백으로 실행
              if (args.SocketError == SocketError.Success)
              {
                  // Accept 성공
                  // 이벤트 Invoke
                  _onAcceptHandler.Invoke(args.AcceptSocket);
              }
              else
              {
                  // Accept 실패
                  Console.WriteLine(args.SocketError.ToString());
              }
              // 다음 턴을 위한 예약
              RegisterAccept(args);
          }
      }
  }
  ```

---
