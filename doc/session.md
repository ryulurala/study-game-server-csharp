---
title: "Session"
category: Game-Server
tags: [c#, socket, server, session, Receive, Send]
date: "2021-02-18"
---

## Session

- `init()`

  > 비동기 완료 콜백 등록  
  > 받을 데이터 버퍼 지정  
  > RegisterReceive()

- `Send()`
  > 한꺼번에 모아서 Send()
  - `Public Send()`
    > 보낼 메시지를 Queue에 계속 Enqueue()  
    > 더 이상 보류된 것이 없을 때 RegisterSend()
  - `RegisterSend()`
    > 보낼 메시지 큐를 비워 한꺼번에 Send() 예약  
    > 비동기 Send() 이므로 예약만 하고 리턴  
    > 만약, 보류 없으면 OnCompletedSend() 호출
  - `OnCompletedSend()`
    > 실질적으로 Send()가 완료됨.  
    > 만약, 보낼 메시지가 큐에 남아있으면 다시 RegisterSend()
- `Receive()`
  - `RegisterRecv()`
    > ReceiveAsync()  
    > 만약 보류 없이 바로 보내지면 OnRecvCompleted
  - `OnCompletedRecv()`
    > 메시지를 다 받음.  
    > 다음 턴을 위해 다시 RegisterRecv()
- `Disconnect()`

  > 두 번 이상 Disconnect() 는 오류이므로, 다른 Thread가 동시에 접근해  
  > Disconnect()를 두 번하는 것을 방지하기 위해 Compare and Swap 이용

- `abstract()`
  > 다른 Session이 상속받아 구현할 함수
  - `OnConnected()`
    > 연결됐을 때 콜백 실행
  - `OnRecv()`
    > 메시지 수신될 떄, 콜백 실행
  - `OnSend()`
    > 메시지 송신 완료할 때, 콜백 실행
  - `OnDisconnected()`
    > 연결이 끊겼을 때 콜백 실행

```cs
abstract public class Session
{
    Socket _socket;     // 클라이언트 소켓(대리자)
    int _disconnected = 0;
    Queue<byte[]> _sendQueue = new Queue<byte[]>();   // 보낼 메시지를 모아서 보냄
    SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();    // 재사용
    SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();    // 재사용
    List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
    object _lock = new object();

    // 다른 session이 구현할 함수
    public abstract void OnConnected(EndPoint endPoint);
    public abstract void OnRecv(ArraySegment<byte> buffer);
    public abstract void OnSend(int numOfBytes);
    public abstract void OnDisconnected(EndPoint endPoint);

    public void init(Socket socket)
    {
        _socket = socket;

        // 비동기 완료 콜백 등록
        _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
        _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

        // argument buffer 설정
        // SetBuffer(buffer, offset, count)
        _recvArgs.SetBuffer(new byte[1024], 0, 1024);

        // 등록(예약)
        ResgisterRecv();
    }
    public void Send(byte[] sendBuff)
    {
        lock (_lock)
        {
            // 쓰레드 하나씩
            _sendQueue.Enqueue(sendBuff);

            if (_pendingList.Count == 0) RegisterSend();
        }
    }
    public void Disconnect()
    {
        // Disconnect를 두 번 하면 Error 발생하므로
        // 멀티쓰레드 환경에서는 이미 Disconnect 한 쓰레드는 접근 X
        // 이미 Disconnect 한 쓰레드가 1을 만드는 것 방지.
        // original value가 1일 경우 close() 불가
        if (Interlocked.Exchange(ref _disconnected, 1) == 1) return;

        OnDisconnected(_socket.RemoteEndPoint);

        // 손님 보내기: Close()
        _socket.Shutdown(SocketShutdown.Both);   // 신뢰성(TCP)
        _socket.Close();
    }

    #region 네트워크 통신
    // 내부적이므로 region 설정
    void RegisterSend()
    {
        // Send()에서 lock을 걸기 때문에 따로 lock 걸 필요 X
        while (_sendQueue.Count > 0)
        {
            byte[] buff = _sendQueue.Dequeue();

            // ArraySegment는 C#에서 구조체로, Stack을 이용 - 효율적
            _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
        }

        // 예약된 목록들
        _sendArgs.BufferList = _pendingList;

        // 여러 번 SendAsync() 호출 시에 부하가 심하다.
        // 예약 확인하며 _sendArgs가 이벤트로 발생시켜 실행한다.
        bool pending = _socket.SendAsync(_sendArgs);
        if (pending == false)
        {
            OnSendCompleted(null, _sendArgs);
        }
    }
    void OnSendCompleted(object sender, SocketAsyncEventArgs args)
    {
        // init() 에서 들어오는 쓰레드가 있으니 Lock을 건다.
        lock (_lock)
        {
            // 상대방이 연결을 끊으면 가끔 0 byte로 온다.
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // 다른 쓰레드 Enqueue 했던 것을 처리
                    // pendingList를 가지고 있을 필요 X
                    _sendArgs.BufferList = null;
                    _pendingList.Clear();
                    OnSend(_sendArgs.BytesTransferred);

                    if (_sendQueue.Count > 0)
                    {
                        // Queue에 남아있으면 그 쓰레드가 Enqueue한 것을 내가 처리
                        RegisterSend();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnSendCompleted Failed {e.ToString()}");
                }
            }
            else
            {
                // TODO Disconnect
                Disconnect();
            }
        }
    }
    void ResgisterRecv()
    {
        bool pending = _socket.ReceiveAsync(_recvArgs);
        if (!pending)
        {
            // 보류 없이 Receive() 성공
            OnRecvCompleted(null, _recvArgs);
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
                OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));

                // 다시 받을 준비
                ResgisterRecv();
            }
            catch (Exception e)
            {
                // 에러 처리
                Console.WriteLine($"OnRecvCompleted Failed {e.ToString()}");
            }
        }
        else
        {
            Disconnect();
        }
    }
    #endregion
}
```

---
