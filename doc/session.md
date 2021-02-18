---
title: "Session"
category: Game-Server
tags: [c#, socket, server, session]
date: "2021-02-18"
---

## Session

1. Send()
2. Receive()
3. Disconnect()

### Receive

```cs
public class Session
{
    Socket _socket;   // Client Socket
    int _disconnected = 0;

    public void init(Socket socket)
    {
        _socket = socket;

        SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();

        // 비동기 이벤트 콜백 등록
        recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

        // argument buffer 설정
        // SetBuffer(buffer, offset, count)
        recvArgs.SetBuffer(new byte[1024], 0, 1024);

        // 등록(예약)
        ResgisterRecv(recvArgs);
    }

    public void Send(byte[] sendBuff)
    {
        _socket.Send(sendBuff);     // blocking
    }

    public void Disconnect()
    {
        // Disconnect를 두 번 하면 Error 발생하므로
        // 멀티쓰레드 환경에서는 이미 Disconnect 한 쓰레드는 접근 X
        // 이미 Disconnect 한 쓰레드가 1을 만드는 것 방지.
        // original value가 1일 경우 close() 불가
        if (Interlocked.Exchange(ref _disconnected, 1) == 1) return;

        // 손님 보내기: Close()
        _socket.Shutdown(SocketShutdown.Both);   // 신뢰성(TCP)
        _socket.Close();
    }

    #region 네트워크 통신
    // 내부적이므로 region 설정
    void ResgisterRecv(SocketAsyncEventArgs args)
    {
        bool pending = _socket.ReceiveAsync(args);
        if (!pending)
        {
            // 보류 없이 Receive() 성공
            OnRecvCompleted(null, args);
        }
    }

    void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
    {
        if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
        {
            // 성공적으로 Data를 가져옴
            // 상대방이 연결을 끊으면 가끔 0 byte로 옴.
            try
            {
                // (buffer, offset=시작 위치, 받은 byte 수)
                string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                Console.WriteLine($"[From Client] {recvData}");

                // 다시 받을 준비
                ResgisterRecv(args);
            }
            catch (Exception e)
            {
                // 에러 처리
                Console.WriteLine($"OnRecvCompleted Failed {e.ToString()}");
            }
        }
        else
        {
            // TODO Disconnect
            Disconnect();
        }
    }
    #endregion
}
```

---
