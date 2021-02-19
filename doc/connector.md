---
title: "connector"
category: Game-Server
tags: [c#, connector]
date: "2021-02-19"
---

## Connector

- 공식적인 명칭 X
- |  Server  |  Client   |
  | :------: | :-------: |
  | Listener | Connector |
- 분산 서버 환경에서는 서버끼리도 통신하기 위해 `Connector`가 필수!

### Connector 예제

```cs
public class Connector
{
    // 어떤 Session인 지 모르므로 Contents 단에서 만든 Session을 Invoke()
    Func<Session> _sessionFactory;

    public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory)
    {
        _sessionFactory = sessionFactory;

        // 휴대폰 설정: Socket()
        Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // For. Async
        SocketAsyncEventArgs args = new SocketAsyncEventArgs();

        // 완료 이벤트 등록
        args.Completed += OnConnectCompleted;
        args.RemoteEndPoint = endPoint;

        // 넘겨줄 정보를 담음.
        // object type으로 받으므로 모든 type이 가능
        args.UserToken = socket;

        // 예약
        RegisterConnect(args);
    }

    void RegisterConnect(SocketAsyncEventArgs args)
    {
        // object형이므로 Casting
        Socket socket = args.UserToken as Socket;
        if (socket == null) return;

        bool pending = socket.ConnectAsync(args);
        if (pending == false)
            OnConnectCompleted(null, args);
    }
    void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            // Contents 단에서 요구하는 방식대로 Session을 생성
            Session session = _sessionFactory.Invoke();
            session.Start(args.ConnectSocket);  // 연결된 Socket

            session.OnConnected(args.RemoteEndPoint);
        }
        else
        {
            Console.WriteLine($"OnConnectCompleted Fail: {args.SocketError}");
        }
    }
}
```

---
