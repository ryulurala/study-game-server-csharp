---
title: "쓰레드 생성"
category: Game-Server
tags: [thread, threadpool, task]
date: "2021-02-04"
---

## 쓰레드 생성

### 쓰레드 생성 방법 3가지

1. Thread
   > 기본적인 Thread 생성
2. ThreadPool
   > 일을 할 Thread 갯수를 지정하고 미리 생성 후 할당  
   > 재사용성으로 효율 좋음  
   > 각자 어떤 일을 하는 Thread인 지 모른다.(static으로 실행)  
   > 최대 Thread 갯수를 넘으면 먹통이 되는 단점
3. Task
   > 기본적으로 ThreadPool에 포함됨.  
   > Option을 지정해 별도의 Thread로 생성 가능

### Thread

- 순서

  1. `using System.Threading;`
  2. `Thread 변수명 = new Thread(call-back);`
  3. `변수명.start();`

- `변수명.join();`
  > 해당 thread가 종료될 때까지 기다림.
- `변수명.Name="...";`
  > 해당 thread의 이름을 지정
- `변수명.IsBackground=bool;`
  > 기본적으로 fore-ground thread(false)이며, back-ground thread로 지정할 때 사용.

```cs
using System;
using System.Threading; // namesapce 필요

namespace core
{
    class Program
    {
        static void TestThread()
        {
            for (int i = 0; i < 5; i++)
                System.Console.WriteLine("Hello Thread!");
        }
        static void Main(string[] args)
        {
            Thread thread = new Thread(TestThread); // TestThread를 실행할 Thread 생성
            thread.Name = "Test Thread";    // Thread의 name을 지정
            thread.IsBackground = true; // Default: false
            thread.Start(); // Thread 시작

            Console.WriteLine("Waiting for thread");

            thread.Join();  // 스레드가 끝날 때까지 기다림.

            // C#은 C++과 달리, 기본적으로 Fore-ground Thread로 생성됨.
            // Back-ground Thread는 Main Thread가 종료되면 바로 종료.
            Console.WriteLine("Hello World!");
        }
    }
}
```

### ThreadPool

- 순서

  1. `using System.Threading;`
  2. `ThreadPool.QueueUserWorkItem(call-back(Object object));`

- `ThreadPool.SetMinThreads(workerThreads, completionPortThreads);`
  > workerThreads: 최소 생성할 Thread 갯수  
  > completionPortThreads: 최소 생성할 IO 관련된 네트워크 이벤트 Thread 갯수
- `ThreadPool.SetMaxThreads(workerThreads, completionPortThreads);`
  > workerThreads: 최대 생성할 Thread 갯수  
  > completionPortThreads: 최대 생성할 IO 관련된 네트워크 이벤트 Thread 갯수

```cs
using System;
using System.Threading; // namespace 필요

namespace core
{
    class Program
    {
        static void TestThread(Object state)  // 매개변수 필요
        {
            for (int i = 0; i < 5; i++)
                System.Console.WriteLine("Hello ThreadPool!");
        }
        static void Main(string[] args)
        {
            // (worker thread 갯수, IO 관련된 네트워크 이벤트 thread 갯수)
            ThreadPool.SetMinThreads(1, 1); // 최소 1개 지정
            ThreadPool.SetMaxThreads(5, 5); // 최대 5개 지정
            ThreadPool.QueueUserWorkItem(TestThread);   // TestThread code를 실행하는 Queue 생성

            Console.WriteLine("Hello World!");
        }
    }
}
```

### Task

- 순서

  1. `using System.Threading.Tasks;`
  2. `Task 변수명 = new Task(action, option);`
  3. `변수명.start();`

- TaskCreationOptions.LongRunning
  > 별도의 Thread를 생성함(오래 걸린다는 것을 알려주는 옵션)

```cs
using System;
using System.Threading.Tasks;   // namespace 필요

namespace core
{
    class Program
    {
        static void Main(string[] args)
        {
            // 옵션으로 작업 시간이 긴 task로 지정
            Task task = new Task(() => { System.Console.WriteLine("Hello Task!"); }, TaskCreationOptions.LongRunning);
            task.Start();   // task 시작

            Console.WriteLine("Hello World!");
        }
    }
}
```

---
