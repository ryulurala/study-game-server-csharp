---
title: "메모리 베리어(H/W 코드 최적화)"
category: Game-Server
tags: [code-optimization, Memory-barrier]
date: "2021-02-08"
---

## H/W 코드 최적화

- 컴파일러(S/W)말고도 H/W도 코드 최적화를 한다.(스케줄링 - 순서를 바꾼다)
- volatile(S/W 코드 최적화) vs Memory Barrier(H/W 코드 최적화)

### Memory Barrier

- 효과

  1. 코드 재배치 억제
  2. 가시성 : 동기화(실제 메모리에 Flush)

- 종류

  |               |  `Full Memory Barrier`   | `Store Memory Barrier` | `Load Memory Barrier` |
  | :-----------: | :----------------------: | :--------------------: | :-------------------: |
  |               | `Store`/`Load` 모두 막음 |     `Store`만 막음     |     `Load`만 막음     |
  | ASM(어셈블리) |         `MFENCE`         |        `SFENCE`        |       `LFENCE`        |
  |      C#       | `Thread.MemoryBarrier()` |                        |                       |

### `Memory Barrier` 예제

```cs
using System;
using System.Threading;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static int x = 0;
        static int y = 0;
        static int r1 = 0;
        static int r2 = 0;

        static void Thread_1()
        {
            y = 1;    // Store y
            r1 = x;     // load x
        }

        static void Thread_2()
        {
            x = 1;    // Store x
            r2 = y;     // load y
        }

        static void Main(string[] args)
        {
            int count = 0;
            while (true)
            {
                count++;
                x = y = r1 = r2 = 0;
                Task t1 = new Task(Thread_1);
                Task t2 = new Task(Thread_2);

                t1.Start();
                t2.Start();

                // Task.WaitAll(Task[]);  // ',' 구분자로 기다릴 task 명시
                Task.WaitAll(t1, t2);

                if (r1 == 0 && r2 == 0) break;
            }
            Console.WriteLine($"{count}번만에 break");
        }
    }
}

```

- Memory Barrier 사용
  > 코드 재배치 억제  
  > 가시성 확보  
  > Store 후의 Memory Barrier, Load 전에 Memory Barrier가 필요

```cs
static void Thread_1()
{
    y = 1;    // Store y

    Thread.MemoryBarrier();

    r1 = x;     // load x
}

static void Thread_2()
{
    x = 1;    // Store x

    Thread.MemoryBarrier();

    r2 = y;     // load y
}
```

---
