---
title: "Thread Local Storage"
category: Game-Server
tags: [thread-local-storage, tls]
date: "2021-02-13"
---

## Thread Local Storage

- `Thread`의 고유한 전역 변수
  > 한 번 Lock을 잡을 때, 일감을 뭉텅이로 가져갈 수 있다.  
  > 공유 자원에 대해 `Thread`들의 접근하는 횟수를 줄일 수 있다.

### TLS 사용[X] 예제

- Code

  ```cs
  static string ThreadName;   // 모든 Thread가 공유

  static void WhoAmI()
  {
      // 자신만의 영역에서 Value 바꾸기
      ThreadName = $"My Name is {Thread.CurrentThread.ManagedThreadId}";

      Thread.Sleep(1000);     // 다른 쓰레드가 고쳤는지 확인

      Console.WriteLine(ThreadName);
  }

  static void Main(string[] args)
  {
      // ThreadPool.SetMinThreads(1, 1);
      // ThreadPool.SetMaxThreads(3, 3);
      // 병렬 처리: ThreadPool에서 꺼내 쓴다.
      Parallel.Invoke(
          WhoAmI, WhoAmI, WhoAmI,
          WhoAmI, WhoAmI, WhoAmI,
          );
  }
  ```

- Result

  ```bash
  My Name is 1
  My Name is 1
  My Name is 1
  My Name is 1
  My Name is 1
  My Name is 1
  ```

### TLS 사용[O] 예제

- `ThreadLocal<T>`
- Code

  ```cs
  // 자신만의 공간
  static ThreadLocal<string> ThreadName = new ThreadLocal<string>();

  static void WhoAmI()
  {
      // 자신만의 영역에서 Value 바꾸기
      ThreadName.Value = $"My Name is {Thread.CurrentThread.ManagedThreadId}";

      Thread.Sleep(1000);     // 다른 쓰레드가 고쳤는지 확인 용도

      Console.WriteLine(ThreadName.Value);
  }

  static void Main(string[] args)
  {
      // 병렬 처리: ThreadPool에서 꺼내 쓴다.
      Parallel.Invoke(
          WhoAmI, WhoAmI, WhoAmI,
          WhoAmI, WhoAmI, WhoAmI,
          );
  }
  ```

- Result

  ```bash
  # 숫자는 변할 수 있음
  # 대신 고유한 Id
  My Name is 6
  My Name is 8
  My Name is 5
  My Name is 9
  My Name is 1
  My Name is 7
  ```

### TLS Value 설정 예제

- `Lambda`를 이용하여 Value 설정
- Code

  ```cs
  // 자신만의 공간
  static ThreadLocal<string> ThreadName = new ThreadLocal<string>(
      () => $"My Name is {Thread.CurrentThread.ManagedThreadId}"
      );  // Value를 지정

  static void WhoAmI()
  {
      if (ThreadName.IsValueCreated)  // 이미 Name이 있으면
          Console.WriteLine($"{ThreadName.Value}(repeat)");
      else  // Value가 null일 경우
          Console.WriteLine($"{ThreadName.Value}");
  }

  static void Main(string[] args)
  {
      // 개수 설정
      ThreadPool.SetMinThreads(1, 1);
      ThreadPool.SetMaxThreads(3, 3);

      // 병렬 처리: ThreadPool에서 꺼내 쓴다.
      Parallel.Invoke(
          WhoAmI, WhoAmI, WhoAmI,
          WhoAmI, WhoAmI, WhoAmI
          );
  }
  ```

- Result

  ```bash
  # 숫자는 변할 수 있음
  My Name is 5
  My Name is 6
  My Name is 1
  My Name is 4
  My Name is 6(repeat)
  My Name is 4(repeat)
  ```

---
