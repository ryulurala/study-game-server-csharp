---
title: "Socket"
category: Game-Server
tags: [c#, socket, server, client]
date: "2021-02-15"
---

## Socket Programming

### Store vs Server/Client

- |  식당 주인  |              Server               |
  | :---------: | :-------------------------------: |
  | 문지기 고용 |            `Socket()`             |
  | 문지기 교육 | `Bind()`: `IP` + `Port` -> Socket |
  |  영업 시작  |            `Listen()`             |
  |    안내     |            `Accept()`             |

- |         손님          |  Client   |
  | :-------------------: | :-------: |
  |      휴대폰 준비      | Socket()  |
  | 식당 번호로 입장 문의 | Connect() |
  |        대리인         |  Session  |

### Socket API(`C#`)

#### Server-Socket

1. `IP Address`

   > `DNS`(`Domain Name System`)를 이용.

   ```cs
   // Localhost
   string host = Dns.GetHostName();

   // Get: IP 주소 배열
   // GetHostEntry(String hostName)
   IPHostEntry ipHost = Dns.GetHostEntry(host);

   // 분산 서버 중 첫 번째
   IPAddress ipAddr = ipHost.AddressList[0];
   ```

2. `End Point`

   > `IP Address` + `Port`

   ```cs
   // IPEndpoint(IP address, Port)
   IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
   ```

3. `Socket()`

   > 소켓 생성  
   > = 문지기 고용

   ```cs
   // Socket(AddressFamily, SocketType, ProtocolType)
   Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
   ```

4. `Bind()`

   > 소켓 연동  
   > = 문지기 교육

   ```cs
   // Bind(EndPoint)
   listenSocket.Bind(endPoint);
   ```

5. `Listen()`

   > = 영업 시작

   - backlog
     > 동시 접속자에 의한 최대 대기 수  
     > 몇 명이 대기할 지...  
     > backlog를 초과하면 접속자 모두 fail

   ```cs
   // Listen(int backlog)
   // Accept() 되기 전까지 10명 대기
   listenSocket.Listen(10);
   ```

6. `Accept()`

   > 접속 허용  
   > = 손님 안내  
   > Client Socket을 반환  
   > Blocking 함수: 입장할 때까지 무한정 대기

   ```cs
   // Socket Accept()
   Socket clientSocket = listenSocket.Accept();
   ```

7. `Send()` / `Receive()`

   - `Receive()`
     > 패킷 수신  
     > `Overload`된 함수에 따라 반환 값이 다름  
     > Blocking 함수

   ```cs
   // 수신 버퍼
   byte[] recvBuff = new byte[1024];

   // int Receive(byte[] buffer)
   int recvBytes = clientSocket.Receive(recvBuff);

   // byte buffer 디코딩, 규약: UTF-8
   // GetString(buffer, start-index, byte-size)
   string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
   ```

   - `Send()`
     > 패킷 송신  
     > `Overload`된 함수에 따라 반환 값이 다름  
     > BLocking 함수

   ```cs
   // 송신 버퍼: byte buffer 인코딩, 규약: UTF-8
   byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server !");

   // int Send(byte[] buffer)
   int sendBytes = clientSocket.Send(sendBuff);
   ```

8. `Close()`

   > Client 연결 끊기  
   > = 손님 보내기

   ```cs
   // Shutdown(SocketShutdown how): 예고, 신뢰성(TCP)
   clientSocket.Shutdown(SocketShutdown.Both);

   // Close()
   clientSocket.Close();
   ```

#### Client-Socket

1. `IP Address`

   > `DNS`(`Domain Name System`)를 이용.

   ```cs
   // Localhost
   string host = Dns.GetHostName();

   // Get: IP 주소 배열
   // GetHostEntry(String hostName)
   IPHostEntry ipHost = Dns.GetHostEntry(host);

   // 분산 서버 중 첫 번째
   IPAddress ipAddr = ipHost.AddressList[0];
   ```

2. `End Point`

   > `IP Address` + `Port`

   ```cs
   // IPEndpoint(IP address, Port)
   IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
   ```

3. `Socket()`

   > 소켓 생성  
   > = 휴대폰 설정

   ```cs
   // Socket(AddressFamily, SocketType, ProtocolType)
   Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
   ```

4. `Connect()`

   > 접속 요청  
   > = 식당 입장 문의
   > Blocking 함수: 접속 성공까지 무한정 대기, **게임에서 치명적**

   ```cs
   // Connect(EndPoint)
   socket.Connect(endPoint);

   // 연결된 위치
   Console.WriteLine($"Connected to {socket.RemoteEndPoint.ToString()}");
   ```

5. `Send()` / `Receive()`

   - `Send()`
     > 패킷 송신  
     > `Overload`된 함수에 따라 반환 값이 다름  
     > BLocking 함수: 서버 측에서 받을 때까지 무한정 대기, **게임에서 치명적**

   ```cs
   // 송신 버퍼: byte buffer 인코딩, 규약: UTF-8
   byte[] sendBuff = Encoding.UTF8.GetBytes("Hello MMOServer !");

   // int Send(byte[] buffer)
   int sendBytes = socket.Send(sendBuff);
   ```

   - `Receive()`
     > 패킷 수신  
     > `Overload`된 함수에 따라 반환 값이 다름  
     > Blocking 함수: 서버 측에서 올 때까지 무한정 대기, **게임에서 치명적**

   ```cs
   // 수신 버퍼
   byte[] recvBuff = new byte[1024];

   // int Receive(byte[] buffer)
   int recvBytes = socket.Receive(recvBuff);

   // byte buffer 디코딩, 규약: UTF-8
   // GetString(buffer, start-index, byte-size)
   string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
   ```

6. `Close()`

   > 연결 끊기  
   > = 퇴장

   ```cs
   // Shutdown(SocketShutdown how): 예고, 신뢰성(TCP)
   clientSocket.Shutdown(SocketShutdown.Both);

   // Close()
   clientSocket.Close();
   ```

#### Server-Client Socket test

- ![socket-api](/uploads/socket/socket-api.gif)

- Server

  ```cs
  static void Main(string[] args)
  {
      // IP 주소
      string host = Dns.GetHostName();
      IPHostEntry ipHost = Dns.GetHostEntry(host);
      IPAddress ipAddr = ipHost.AddressList[0];

      // Port 번호
      IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

      // 문지기(TCP): Listen Socket 초기화
      Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


      try
      {
          // 문지기 교육: Bind
          listenSocket.Bind(endPoint);

          // 영업시작: Listen
          // backlog: 최대 대기수, 몇 명이 대기할 지... backlog를 초과하면 모두 fail
          listenSocket.Listen(10);    // Accept() 되기 전까지 10명 대기

          while (true)
          {
              Console.WriteLine("Listening...");

              // 손님 안내: Accept()
              Socket clientSocket = listenSocket.Accept();    // Blocking: 다음 단계 불가

              // 메시지 받기: Receive()
              byte[] recvBuff = new byte[1024];
              int recvBytes = clientSocket.Receive(recvBuff);
              string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
              Console.WriteLine($"[From Client] {recvData}");

              // 메시지 보내기: Send()
              byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server !");
              clientSocket.Send(sendBuff);    // Blocking: 다음 단계 불가

              // 손님 보내기: Close()
              clientSocket.Shutdown(SocketShutdown.Both);   // 신뢰성(TCP)
              clientSocket.Close();
          }
      }
      catch (Exception e)
      {
          Console.WriteLine(e);
      }
  }
  ```

- Client

  ```cs
  static void Main(string[] args)
  {
      // IP 주소
      string host = Dns.GetHostName();
      IPHostEntry ipHost = Dns.GetHostEntry(host);
      IPAddress ipAddr = ipHost.AddressList[0];

      // Port 번호
      IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

      // 휴대폰 설정: Socket()

      while (true)
      {
          Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
          try
          {
              // 문지기한테 입장 문의: Connect()
              socket.Connect(endPoint);
              Console.WriteLine($"Connected to {socket.RemoteEndPoint.ToString()}");

              // 메시지 보내기: Send()
              byte[] sendBuff = Encoding.UTF8.GetBytes("Hello MMOServer !");
              int sendBytes = socket.Send(sendBuff);

              // 메시지 받기: Receive()
              byte[] recvBuff = new byte[1024];
              int recvBytes = socket.Receive(recvBuff);
              string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
              Console.WriteLine($"[From Server] {recvData}");

              // 퇴장: Close()
              socket.Shutdown(SocketShutdown.Both);   // 신뢰성(TCP)
              socket.Close();
          }
          catch (Exception e)
          {
              Console.WriteLine(e);
          }
          Thread.Sleep(500);
      }
  ```

---
