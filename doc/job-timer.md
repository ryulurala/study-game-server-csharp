---
title: "Job Timer"
category: Game-Server
tags: [packet, job-timer]
date: "2021-03-04"
---

## Job Timer

### 패킷 모아보내기

- Packet 모아보내기는 Contents, Engine 단 모두 가능
- `Flush()`: `List<>`에 모아서 한꺼번에 `Send()`

#### Main Program Thread

- Contents 단에서 Packet 모아보내기

```cs
while(true)
{
    // Flush(): Packet을 한꺼번에 Send()
    Room.Push(() => Room.Flush());

    // 이후에 Job Timer 설정
    Thread.Sleep(250);
}
```

#### Contents의 Thread

```cs
// 보낼 메시지 모아서 보낼 공간
// ArraySegment<>의 List
List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

public void Flush()
{
    // Session별로 Pending List<>를 Send()
    // 이전에는 ArraySegment<>를 Send()
    foreach (ClientSession s in _sessions)
        s.Send(_pendingList);

    Console.WriteLine($"Flushed {_pendingList.Count} items");
    _pendingList.Clear();
}
```

#### Core의 Thread

```cs
public void Send(List<ArraySegment<byte>> sendBuffList)
{
    // Error 방지: 다른 Thread가 비어있는 sendBuff를 보낼 수도 있기 때문에
    if (sendBuffList.Count == 0)
        return;

    lock (_lock)
    {
        // Pending List에 담겨진 sendBuff를 sendQueue에 Push
        foreach (ArraySegment<byte> sendBuff in sendBuffList)
            _sendQueue.Enqueue(sendBuff);

        // 처음 들어온 Thread가 RegisterSend()
        if (_pendingList.Count == 0)
            RegisterSend();
    }
}
```

---
