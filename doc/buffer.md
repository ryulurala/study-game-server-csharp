---
title: "Buffer"
category: Game-Server
tags: [buffer, receive, send]
date: "2021-02-21"
---

## Buffer

- TCP 통신할 경우, 끊어진 패킷에 대해 부분적으로 처리할 수 있도록 해야 한다.

### Receive Buffer

- `void Clean()`
- `bool OnRead(int numOfBytes)`
  > Read할 경우를 처리  
  > read cursor를 옮김  
  > 2byte 읽을 때: [r][][][][w] -> [][][r][][w]  
  > Error or Success
- `bool OnWrite(int numOfBytes)`
  > Write할 경우를 처리  
  > Write cursor를 옮김  
  > Data가 4byte 들어올 때: [rw][][][][] -> [r][][][][w]  
  > Error or Success

```cs
public class RecvBuffer
{
    // [rw][][][][][][][]
    ArraySegment<byte> _buffer; // buffer, C#은 Segment로 사용(pointer 불가)
    int _readPos; // read cursor
    int _writePos;  // write cursor

    public RecvBuffer(int bufferSize)   // 생성자
    {
        _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
    }

    // Property: Data크기, 남은 공간 크기
    public int DataSize { get { return _writePos - _readPos; } }
    public int FreeSize { get { return _buffer.Count - _writePos; } }

    // 처리할 Segment 리턴
    public ArraySegment<byte> ReadSegment
    {
        get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
    }
    // 남은 공간 Segment 리턴
    public ArraySegment<byte> WriteSegment
    {
        get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
    }

    public void Clean()
    {
        int dataSize = DataSize;
        if (dataSize == 0)
        {
            // 남은 데이터가 없을 경우
            // [rw] 겹쳐 있을 때: 커서 위치만 초기로
            _readPos = _writePos = 0;
        }
        else
        {
            // [][][r][][w] -> [r][][w][][]
            // 남은 데이터가 있을 때: 데이터 크기를 시작 위치로 복사
            // Copy(SourceArray, Source 위치, DestinationArray, Destination위치, 복사할 크기)
            Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
            _readPos = 0;
            _writePos = dataSize;
        }
    }

    public bool OnRead(int numOfBytes)  // Read 할 경우
    {
        if (numOfBytes > DataSize)
            return false;

        // read cursor 이동
        _readPos += numOfBytes;
        return true;
    }

    public bool OnWrite(int numOfBytes) // Write 할 경우
    {
        if (numOfBytes > FreeSize)
            return false;

        // write cursor 이동
        _writePos += numOfBytes;
        return true;
    }
}
```

- in Session.cs

```cs
void ResgisterRecv()
{
    _recvBuffer.Clean();  // 커서가 너무 뒤로 이동하는 것을 방지
    ArraySegment<byte> segment = _recvBuffer.WriteSegment;  // 남은 세그먼트 공간 리턴

    _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

    // ...
}

void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
{
    // ...
    // 성공적으로 Data를 가져옴
    // Write 커서 이동
    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
    {
        // 버그 발생
        Disconnect();
        return;
    }

    // 컨텐츠 단으로 데이터를 넘겨주고 얼마나 처리했는지 받음
    int processLen = OnRecv(_recvBuffer.ReadSegment);
    if (processLen < 0 || _recvBuffer.DataSize < processLen)
    {
        // 버그 발생: 컨텐츠 단에서 이상하게 넣을 경우
        Disconnect();
        return;
    }

    // Read 커서 이동
    if (_recvBuffer.OnRead(processLen) == false)
    {
        // 버그 발생
        Disconnect();
        return;
    }

    ResgisterRecv();
}
```

### Send Buffer

- Send()는 Receive()와 다르게 모든 사용자에게 동일한 상황을 주기 위해 많이 실행된다.
  > Send Buffer를 Copy해서 사용하게 되면 Copy 비용이 많이 들기 때문에 최적화가 필요.  
  > 뭉텅이(Chunk)로 만들고 쪼개서 사용하는 방식으로 최적화.
- Send Buffer는 일회용

  > 다른 Session에서 Send를 위해 Queue에 집어넣은 상태일 수도 있으므로 Clean 불가능

- `ArraySegement<byte> Open(int reserveSize)`
  > 예약된 공간을 리턴
- `ArraySegement<byte> Close(int usedSize)`
  > 실질적으로 사용할 공간을 리턴

```cs
// SendBuffer를 예약과 생성을 도와주는 클래스
public class SendBufferHelper
{
    // Thread의 고유한 전역 공간
    // Thread끼리 경합할 필요 X
    public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => null);
    // 뭉텅이(Chunk)로 할당하고 쪼개서 사용
    public static int ChunkSize { get; set; } = 4096 * 100;

    public static ArraySegment<byte> Open(int reserveSize)
    {
        if (CurrentBuffer.Value == null)
            CurrentBuffer.Value = new SendBuffer(ChunkSize);

        // 해당 구문은 이전 SendBuffer 객체를 지움.
        // Pooling 방식으로 재사용으로 조금 더 최적화 가능.
        if (CurrentBuffer.Value.FreeSize < reserveSize)
            CurrentBuffer.Value = new SendBuffer(ChunkSize);

        return CurrentBuffer.Value.Open(reserveSize);
    }

    public static ArraySegment<byte> Close(int usedSize)
    {
        return CurrentBuffer.Value.Close(usedSize);
    }
}

// 실질적인 Send-Buffer
public class SendBuffer
{
    // [u][][][][][][][]
    byte[] _buffer;     // buffer
    int _usedSize = 0;  // used cursor

    public SendBuffer(int chunkSize)
    {
        _buffer = new byte[chunkSize];
    }

    // Property: 남은 공간 크기
    public int FreeSize { get { return _buffer.Length - _usedSize; } }

    public ArraySegment<byte> Open(int reserveSize)
    {
        if (reserveSize > FreeSize)
            return null;

        return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
    }

    public ArraySegment<byte> Close(int usedSize)
    {
        ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
        _usedSize += usedSize;

        return segment;
    }
}
```

#### Serialize without send-buffer

```cs
class Knight
{
    public int hp;
    public int attack;
}

public override void OnConnected(EndPoint endPoint)
{
    Knight knight = new Knight() { hp = 100, attack = 10 };

    byte[] sendBuff = new byte[1024];
    byte[] buffer1 = BitConverter.GetBytes(knight.hp);  // int -> byte[]
    byte[] buffer2 = BitConverter.GetBytes(knight.attack);  // int -> byte[]

    Array.Copy(buffer1, 0, sendBuff, 0, buffer1.Length);
    Array.Copy(buffer2, 0, sendBuff, buffer1.Length, buffer2.Length);

    Send(sendBuff);
}
```

#### Serialize with send-buffer

```cs
class Knight
{
    public int hp;
    public int attack;
}

public override void OnConnected(EndPoint endPoint)
{
    Knight knight = new Knight() { hp = 100, attack = 10 };

    ArraySegment<byte> openSegment = SendBufferHelper.Open(4096); // 공간 예약
    byte[] buffer1 = BitConverter.GetBytes(knight.hp);
    byte[] buffer2 = BitConverter.GetBytes(knight.attack);

    Array.Copy(buffer1, 0, openSegment.Array, openSegment.Offset, buffer1.Length);
    Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer1.Length, buffer2.Length);

    // 실질적인 버퍼를 부여받음
    ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer1.Length + buffer2.Length);

    Send(sendBuff);
}
```

---
