---
title: "Spinlock"
category: Game-Server
tags: [spinlock, compare-and-swap]
date: "2021-02-10"
---

## SpinLock

- 무작정 기다리기

### SpinLock 예제

- 잘못된 방법

  > 동시에 들어갈 수 있는 여지가 있다.

  ```cs
  class MySpinLock
  {
      volatile int _locked = 0;

      public void Acquire()
      {
          while (_locked) ;   // 잠금이 풀리기를 기다림
          _locked = true;  // 내가 잠금
      }
      public void Release()
      {
          _locked = 0;    // 잠금 해제
      }
  }
  ```

- 올바른 방법

  > Atomic(원자적)하게 실행한다.
  > `CaS`(`Compare and Swap`) 사용.

  ```cs
  class MySpinLock
  {
      volatile int _locked = 0;

      public void Acquire()
      {
          // CaS: Compare-and-Swap
          while (true)
          {
              int expected = 0; // 예상 값
              int desired = 1;  // 원하는 값
              if (Interlocked.CompareExchange(ref _locked, desired, expected) == expected)
                  break;  // 예상값과 맞을 경우 원하는 값으로 넣어주고 break
          }

      }
      public void Release()
      {
          _locked = 0;    // 잠금 해제
      }
  }
  ```

### SpinLock API

- SpinLock API를 사용할 경우 실제로 양보를 하기도 한다.

```cs
// SpinLock API는 실제로 답이 없을 경우 양보를 한다.
static SpinLock spinLock = new SpinLock();
static void Main(string[] args)
{
    bool token = false;
    try
    {
        spinLock.Enter(ref token);
    }
    finally
    {
        if (token) spinLock.Exit();
    }
}
```

---
