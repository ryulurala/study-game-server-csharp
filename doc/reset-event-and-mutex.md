---
title: "AutoResetEvent & Mutex"
category: Game-Server
tags: [auto-reset-event, manual-reset-event, mutex]
date: "2021-02-10"
---

## `AutoResetEvent`

- 관리자(`Kernel`)가 중재
- 일종의 `Kernel`단에서의 `Flag(bool)` 개념이다.
- 속도가 느리다. Why. `Kernel`이 중재

- `AutoResetEvent` vs `ManualResetEvent`

  > 쓰레드를 받아들일 문을 닫는 행위: 자동(Auto) vs 수동(Manual)

  |    `AutoResetEvent`     |         `ManualResetEvent`         |
  | :---------------------: | :--------------------------------: |
  |     `Kernel`이 중재     |          `Kernel`이 중재           |
  | `.WaitOne()` / `.Set()` | `.WaitOne()`+`.Reset()` / `.Set()` |
  |       버그 발생 X       |            버그 발생 O             |

### AutoResetEvent 예제

```cs
class Lock
{
    // 관리자가 중재.
    // true: 들어올 수 있음, false: 못 들어옴
    AutoResetEvent _available = new AutoResetEvent(true);
    public void Acquire()
    {
        _available.WaitOne();   // 입장 시도 후 문을 닫음
    }
    public void Release()
    {
        _available.Set();   // 문을 열음, state = true;
    }
}
```

### ManualResetEvent 예제

```cs
class Lock
{
    // true: 들어올 수 있음, false: 못 들어옴
    // 관리자가 중재.
    ManualResetEvent _available = new ManualResetEvent(true);
    public void Acquire()
    {
        // AutoResetEvent는 두 절을 묶어서 사용
        // ManualResetEvent는 나뉘어서 실행하므로 문제가 발생할 수 있다.
        _available.WaitOne();   // 입장 시도
        _available.Reset();     // 입장 후 문을 닫음
    }
    public void Release()
    {
        _available.Set();   // 문을 열음.
    }
}
```

## `Mutex`

- Kernel 동기화 객체
- `ResetEvent`보다 많은 `State`(`Thread Id`, `Lock count`, ... 등등)를 담고 있다
- `ResetEvent` 보다 조금 더 느리다.
- Mutex 객체를 생성하여 이용한다.
- Kernel 단에서 동기화하므로 `Process`(프로그램)끼리 동기화가 가능하다.

### Mutex 예제

```cs
static int _num = 0;
static Mutex _lock = new Mutex();

static void Thread_1()
{
    for (int i = 0; i < 10000; i++)
    {
        _lock.WaitOne();    // 입장 시도 및 잠그기
        _num++;
        _lock.ReleaseMutex();   // 잠금 해제
    }
}
static void Thread_2()
{
    for (int i = 0; i < 10000; i++)
    {
        _lock.WaitOne();    // 입장 시도 및 잠그기
        _num--;
        _lock.ReleaseMutex();   // 잠금 해제
    }
}
```

---
