---
title: "lock 종류"
category: Game-Server
tags: [spinlock, context-switching, auto-reset-event, manual-reset-event, mutex]
date: "2021-02-09"
---

## Lock 종류

1. `SpinLock`
   - 무한정 기다리기.
   - 내 일을 하지 못함.
   - `C#`의 `SpinLock`은 무한정 기다리지 않는다.
2. `Context Switching`
   - 내 일을 하다 나중에 다시 옴.
   - 다시 와도 이미 누군가가 소유권을 잡은 상태일 수도 있음.(= 랜덤)
3. `AutoResetEvent` or `ManualResetEvent` / `Mutex`
   - 관리자가 중재.
   - 관리자에게 비용이 들어감.

### `C#`의 `Lock`

|           근성            |       양보        |          중재          |
| :-----------------------: | :---------------: | :--------------------: |
|      무한정 기다리기      |  잠깐 쉬다 오기   |     관리자가 중재      |
| `lock(Object obj){ ... }` |  `SpinLock API`   |  `AutoResetEvent API`  |
|                           | `Thread.Sleep(N)` | `ManualResetEvent API` |
|                           | `Thread.Sleep(0)` |      `Mutex API`       |
|                           | `Thread.Yield()`  |                        |

---
