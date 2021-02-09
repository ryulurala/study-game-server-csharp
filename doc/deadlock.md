---
title: "데드락(DeadLock)"
category: Game-Server
tags: [lock, deadlock]
date: "2021-02-09"
---

## 데드락(DeadLock)

### `DeadLock`

- 교착 상태: 서로 상대방의 작업이 끝나기를 무한정 기다리는 상태

### `DeadLock` 발생 조건

- 4 가지 모두 만족.
- 하나라도 만족 안할 시에 `DeadLock`발생 X
  1. `Mutual exclusion`: 공유 자원이 상호 배제한 상태로 사용될 때
  2. `Hold and wait`: 잡으려는 것과 기다리는 것이 동시에 존재
  3. `No preemption`: 데드락 상태에서 자원 선점(뺏기)이 없어야 함.(우선순위 X)
  4. `Circular wait`: `Resource-allocation graph`(자원 할당 그래프)가 순환이 발생

### `DeadLock` 해결 방법

- `Prevention`: 미리 방지

  > 4가지 필요 조건을 하나라도 불만족하도록 한다.  
  > 발생 조건 (1), (3)은 불가  
  > 발생 조건 (2)는 가능: 한꺼번에 요청 - But. 성능 저하가 있다.  
  > 발생 조건 (4)는 가능(제일 현실적): 자원을 `orderly`(순서 있게) 요청하도록 한다.

- `Avoidance`: 자원요청 시 검사

  > `safe state`, `unsafe state` 검사  
  > 요청마다 처리해야하므로 오버헤드가 크다.

- `Detection & Recovery`: 데드락 발생 후에 처리
  > 미리 부여한 우선순위로 데드락을 처리.  
  > Rollback & Retry: Safe state를 표시했던 곳으로 되돌아 간다.

### `DeadLock` 예제

```cs
using System;
using System.Threading;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        class SessionManager
        {
            static object _lock = new object();
            public static void TestSession()
            {
                lock (_lock) { }
            }
            public static void Test()
            {
                lock (_lock) { UserManager.TestUser(); }
            }
        }

        class UserManager
        {
            static object _lock = new object();

            public static void TestUser()
            {
                lock (_lock) { }
            }
            public static void Test()
            {
                lock (_lock) { SessionManager.TestSession(); }
            }
        }


        static void Thread_1()
        {
            for (int i = 0; i < 10000; i++)
            {
                SessionManager.Test();
            }
        }
        static void Thread_2()
        {
            for (int i = 0; i < 10000; i++)
            {
                UserManager.Test();
            }
        }

        static void Main(string[] args)
        {
            Task task1 = new Task(Thread_1);
            Task task2 = new Task(Thread_2);
            task1.Start();
            task2.Start();
            Task.WaitAll(task1, task2);

            Console.WriteLine("종료!");
        }
    }
}
```

- 해결 방법

```cs
static void Main(string[] args)
{
    Task task1 = new Task(Thread_1);
    Task task2 = new Task(Thread_2);
    task1.Start();
    Thread.Sleep(100);  // Orderly 하게 실행함
    task2.Start();
    Task.WaitAll(task1, task2);

    Console.WriteLine("종료!");
}
```

---
