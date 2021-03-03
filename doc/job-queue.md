---
title: "Job Queue"
category: Game-Server
tags: [command-pattern, job, task, queue, lock]
date: "2021-03-02"
---

## Job(or Task) Queue

- 일감을 바로 처리하지 않고 큐에 넣고 나중에 처리한다.

- 바로 처리

  ```cs
  clientSession.Room.BroadCast(clientSession, chatPacket.chat);
  ```

- Queue에 넣고 나중에 처리

  ```cs
  // 나중에 처리하기 때문에 미리 참조
  // Room이 null이 되는 상황
  GameRoom room = clientSession.Room;
  room.Push(
      () => room.BroadCast(clientSession, chatPacket.chat)
  );
  ```

### Command Pattern

- 요청을 객체의 형태로 캡슐화하여 사용자가 보낸 요청을 나중에 이용할 수 있도록 하는 디자인 패턴
- |          Command Pattern          |                식당                |
  | :-------------------------------: | :--------------------------------: |
  |            요청 + 처리            |       주문 받기 + 주문 처리        |
  |            객체 캡슐화            |               주문서               |
  | 요청을 받으면 `Queue`에 넣고 처리 | 주문을 받으면 주문서 순서대로 처리 |

### `JobQueue.cs`

- Lambda 문법으로 Call-back으로 일감 넘기기 가능
- `JobQueue.cs`에서 Lock을 걸고 처리하기 때문에 Call-back func에 대해서 Lock처리가 필요 없는 장점

  > Call-back 넘기기 어렵다면 Task(class) 정의

- Job Queue 처리 과정
  1. JobQueue 객체를 생성해 모든 일감을 Queue에 Push()
     > C#은 Action을 이용해 Call-back으로 실행되도록
  2. 첫 번째 들어온 Thread가 모든 일감을 처리하도록 Flush() 로직 구성
     > `While(true) Pop();`

```cs
// 인터페이스: Push()는 항상 가지고 있도록
public interface IJobQueue
{
    void Push(Action job);
}

public class JobQueue : IJobQueue
{
    // 해야 할 일 모음
    Queue<Action> _jobQueue = new Queue<Action>();
    object _lock = new object();
    bool _flush = false;

    public void Push(Action job)
    {
        bool flush = false;
        lock (_lock)
        {
            // 큐에 일감 넣기
            _jobQueue.Enqueue(job);

            if (_flush == false)
                flush = _flush = true;
        }

        if (flush)
            Flush();
    }

    void Flush()
    {
        // 처음 들어온 쓰레드가 모든 일감 처리
        while (true)
        {
            Action action = Pop();
            if (action == null)
                return;

            // 일감 실행
            action.Invoke();
        }
    }

    Action Pop()
    {
        lock (_lock)
        {
            if (_jobQueue.Count == 0)
            {
                // 모든 일감이 끝나면 다른 쓰레드가 일감 처리 할 수 있도록
                _flush = false;
                return null;
            }

            return _jobQueue.Dequeue();
        }
    }
}
```

### `TaskQueue.cs`

- 처리할 일들을 객체 형태로 정의

- Task Queue 처리 과정

  1. 일감마다 Class를 만들어 객체 형태 생성
  2. 객체를 ITask로 가리켜 Task Queue에 넣음
  3. Task Queue에서 빼낸 일감을 Execute()

```cs
// 인터페이스: Execute()는 무조건 가지고 있도록
interface ITask
{
    void Execute();
}

// 일감마다 Class를 만들어 객체로 생성
class BroadCastTask : ITask
{
    GameRoom _room;
    ClientSession _session;
    string _chat;

    BroadCastTask(GameRoom room, ClientSession session, string chat)
    {
        _room = room;
        _session = session;
        _chat = chat;
    }

    public void Execute()
    {
        // Broadcasting 일감 실행
        _room.BroadCast(_session, _chat);
    }
}
class TaskQueue
{
    // 객체로 생성된 일감을 Task Queue에 넣음
    Queue<ITask> _queue = new Queue<ITask>();

    // Queue에서 일감을 빼내어 Execute() 실행
}
```

---
