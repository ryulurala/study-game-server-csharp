---
title: "Context Switching"
category: Game-Server
tags: [context-switching, sleep, yield]
date: "2021-02-10"
---

## Context Switching

- 무작정 기다리지 않고 "내 일을 하다가 다시 올게".

### Context Switching 절차

1. Sleep()을 실행하면 CPU 점유권을 포기하고 커널 모드로 들어가 OS에게 실행 권한을 넘겨준다.
2. OS가 그 다음 실행할 쓰레드를 결정 : 오래 기다린 순서 + 중요도(Initialized)
3. Context-Switching(by Register)
   1. `Register`에 들어있는 `State`를 `RAM`에 저장
   2. `Switching`
   3. `Register`에 들어올 값 복원
4. 복원 후, 다시 이어서 실행

- 모든 프로세스는 CPU Time Slice를 간절하기 받기를 원하는 상태이다.
- 실제로 남는 쓰레드가 거의 없다.
  > Why. 모든 Thread가 Sleep 상태가 아닌 이상, 실제 프로세스 개수를 생각하자!

### Context Switching API

|        `Thread.Sleep(N)`         |                   `Thread.Sleep(0)`                    |                   `Thread.Yield()`                   |
| :------------------------------: | :----------------------------------------------------: | :--------------------------------------------------: |
|           무조건 휴식            |                      조건부 양보                       |                     관대한 양보                      |
| 실제로는 `OS`에게 N ms 휴식 요청 | 우선순위 높은 `Thread`에게 양보, 없으면 다시 본인 실행 | 실행 가능한 `Thread`에게 양보, 없으면 남은 시간 소진 |

### Context-Switching 예제

```cs
public void Acquire()
{
    // CaS: Compare-and-Swap
    while (true)
    {
        int expected = 0;
        int desired = 1;
        if (Interlocked.CompareExchange(ref _locked, desired, expected) == expected)
            break;
    }

    // Context-Switching(3개 중 하나 실행)
    Thread.Sleep(1);    // 무조건 휴식, (N)ms 쉬게 함.
    Thread.Sleep(0);    // 조건부 양보, 나보다 우선순위가 높은 애들한테는 양보 or 아니면 다시 실행
    Thread.Yield();     // 관대한 양보, 지금 실행 가능한 Thread가 있으면 실행 or 없으면 다시 실행
}
```

---
