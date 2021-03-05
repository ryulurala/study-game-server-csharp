---
title: "Send pending packet & Job Timer"
category: Game-Server
tags: [pending-packet, priority_queue, job-timer, bucket]
date: "2021-03-04"
---

## 패킷 모아보내기

- Server Thread의 병목 현상을 감소시켜준다.
- Packet 모아보내기는 Contents, Engine 단 모두 가능
- `Flush()`: `List<>`에 모아서 한꺼번에 `Send()`

### Main Program Thread

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

### Contents의 Thread

```cs
// 보낼 메시지 모아서 보낼 공간, 한꺼번에 Send
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

public void BroadCast(ClientSession session, string chat)
{
    S_Chat packet = new S_Chat();
    packet.playerId = session.SessionId;
    packet.chat = $"{chat} I am {packet.playerId}";
    ArraySegment<byte> segment = packet.Write();

    // Reserve Send
    // 바로 Send() X
    // _pendingList에 넣어놓고 한꺼번에 Send()
    _pendingList.Add(segment);
}
```

### Core의 Thread

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

## 1. Job timer

- Server가 할 일을 일정 시간 주기로 일을 행하도록 한다.
  - log를 파일로 기록
    > Crash 났을 경우, crash dump 기록
  - Send()

```cs
static void Main(string[] args)
{
    while(true)
    {
        Room.Push(() => Room.Flush());

        // 250ms 마다 Room Flush()
        Thread.Sleep(250);
    }
}
```

### Main Program에서 Tick 처리

```cs
static void Main(string[] args)
{
    int roomTick = 0;
    // ...Tick1 = 0;
    // ...Tick2 = 0;
    // ...

    while (true)
    {
        // 현재 Tick
        int now = System.Environment.TickCount;

        if (roomTick < now)
        {
            // Job
            Room.Push(() => Room.Flush());

            // 250ms 마다 Room Flush()
            roomTick = now + 250;
        }

        // if(...Tick1 < now)
        // {
            // 할 일(= job)

            // Nms 마다 job
            // ...Tick1 = now + N1;
        // }

        // TODO

        // TODO

    }
}
```

### 2. JobTimer class에서 처리

- 우선순위 큐로 구현

#### Priority Queue

- `Generic<T>`으로 `IComparable<T>`를 정의한 것을 Queue의 Element로 사용

```cs
public class PriorityQueue<T> where T : IComparable<T>
{
    List<T> _heap = new List<T>();
    public int Count { get { return _heap.Count; } }

    // O(log N)
    public void Push(T data)
    {
        // Heap의 맨 끝에 새로운 Data 삽입
        _heap.Add(data);

        int now = _heap.Count - 1;

        // 위치 set
        while (now > 0)
        {
            int next = (now - 1) / 2;

            // 현재 값이 다음 값보다 작을 경우 실패
            if (_heap[now].CompareTo(_heap[next]) < 0)
                break;

            // 두 값 교체
            T temp = _heap[now];
            _heap[now] = _heap[next];
            _heap[next] = temp;

            // 검사 위치 이동
            now = next;
        }
    }

    // O(log N)
    public T Pop()
    {
        // 반환할 Data 저장
        T ret = _heap[0];

        // 마지막 Data를 Root로 이동
        int lastIndex = _heap.Count - 1;
        _heap[0] = _heap[lastIndex];
        _heap.RemoveAt(lastIndex);
        lastIndex--;

        // 역으로 검사
        int now = 0;
        while (true)
        {
            int left = 2 * now + 1;
            int right = 2 * now + 2;

            int next = now;
            // 왼쪽 값이 현재 값보다 크면 왼쪽으로 이동
            if (left <= lastIndex && _heap[next].CompareTo(_heap[left]) < 0)
                next = left;

            // 오른쪽 값이 현재 값(왼쪽 이동 포함)보다 크면 오른쪽으로 이동
            if (right <= lastIndex && _heap[next].CompareTo(_heap[right]) < 0)
                next = right;

            // 왼쪽/오른쪽 모두 현재값보다 작으면 종료
            if (next == now)
                break;

            // 두 값을 교체
            T temp = _heap[now];
            _heap[now] = _heap[next];
            _heap[next] = temp;

            // 검사 위치 이동
            now = next;
        }

        return ret;
    }

    // 엿보기
    public T Peek()
    {
        if (_heap.Count == 0)
            return default(T);  // value type은 0, reference type은 null

        // 가장 좋은 Value
        return _heap[0];
    }
}
```

#### JobTimer.cs

```cs
struct JobTimerElem : IComparable<JobTimerElem>
{
    public int execTick;    // 실행 시간
    public Action action;   // 실행 일감 Call-back

    // Queue의 Element로 사용하기 위한 CompareTo<T> 정의
    public int CompareTo(JobTimerElem other)
    {
        // Tick 작은 것이 먼저 오도록
        return other.execTick - execTick;
    }
}

class JobTimer
{
    PriorityQueue<JobTimerElem> _pq = new PriorityQueue<JobTimerElem>();
    object _lock = new object();

    // Singleton
    public static JobTimer Instance { get; } = new test.JobTimer();

    // 몇 Tick 후에 Call-back func call 할 지
    public void Push(Action action, int tickAfter = 0)
    {
        JobTimerElem job;
        job.execTick = System.Environment.TickCount + tickAfter;
        job.action = action;

        lock (_lock)
        {
            _pq.Push(job);
        }
    }

    public void Flush()
    {
        while (true)
        {
            int now = System.Environment.TickCount;

            JobTimerElem job;
            lock (_lock)
            {
                if (_pq.Count == 0)
                    break;

                // 실행할 Job인지 확인
                job = _pq.Peek();
                if (job.execTick > now) // 아직인지
                    break;

                _pq.Pop();
            }

            job.action.Invoke();  // 일감을 실행(call-back)
        }
    }
}
```

#### Main Program

- 등록 + `Flush()`로 Main Program 실행

```cs
public static GameRoom Room = new GameRoom();

static void FlushRoom()
{
    Room.Push(() => Room.Flush());

    // 250ms 이후에 다시 실행: 예약
    JobTimer.Instance.Push(FlushRoom, 250);
}

static void Main(string[] args)
{
    // 등록
    JobTimer.Instance.Push(FlushRoom, 250);

    while (true)
    {
        // Job Flush
        JobTimer.Instance.Flush();
    }
}
```

### 3. Bucket을 이용한 최적화

- JobTimer System + Bucket System
  > Bucket 단위로 비슷한 시간대에 실행할 Job 뭉텅이 이용
  - [`20ms`][`20ms`][`...`][`...`]의 List로 관리하여 더 효율적인 예약 시스템
    > (...) 안에는 연결 리스트로 돼있다.

---
