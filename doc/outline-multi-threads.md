---
title: "멀티쓰레드 개론"
excerpt: "Process vs Thread"
category: Game-Server
tags: [process, thread, multi-thread, context-switching, thread-safe]
toc: true
---

## 멀티쓰레드 개론

### Process vs Thread

|                           Process                           |                    Thread                     |
| :---------------------------------------------------------: | :-------------------------------------------: |
|      CPU 자원을 할당 받은 작업 단위(=하나의 프로그램)       |              Process의 실행 단위              |
|                Code, Data, Heap, Stack 할당                 |            Stack, PC 레지스터 할당            |
|                 IPC 통신 필요(Pipe, Socket)                 |       Process내의 Code, Data, Heap 공유       |
|                Context Switching 오버헤드 큼                |        Context Switching 오버헤드 작음        |
| 1개의 Process 문제가 전체 Process에 영향이 모두 가지는 않음 | 1개의 Thread의 문제가 전체 Thread에 영향이 감 |

- Code, Data, Heap, Stack
  - Code: 기계어 Code가 저장됨.(Read-Only)
  - Data: 전역 변수, Static 변수가 저장됨.(초기화되지 않으면 BSS 영역)
  - Heap: 동적으로 메모리를 생성하며 유일하게 Runtime에 결정됨.
  - Stack: 지역 변수(매개 변수), 리턴값, 리턴주소값 등 저장됨.(함수 호출 시 기록, 리턴 시 제거)
- PC(Program Counter) 레지스터
  > Thread의 명령어가 어디까지 수행했는지를 나타냄  
  > Preemption(자원 선점)으로 인한 명령어가 연속적으로 수행하지 못하므로 독립적으로 필요함.
- Context Switching
  > CPU는 한 번에 하나의 Process만 처리하므로 여러 Process를 처리할 때, 다른 Process에게 CPU를 할당해 작업을 수행하는 과정을 말함.  
  > 중지할 Process 상태는 PCB(Process Control Block)에 저장하고 진행할 Task는 PCB에서 읽는다.

### 멀티쓰레드 사용 이유

1. 프로그램(Process)을 여러 개 키는 것보다 하나의 프로그램 안에서 여러 작업을 해결하는 것이 더 효율적
2. Process를 생성하여 자원을 할당하는 System-Call이 줄어들어 자원을 효율적으로 관리 가능
3. Context Switching 비용이 적고 빠르다.(Stack만 비우면 되기 때문에??)

### Thread-Safe

- 멀티쓰레드 환경에서 여러 스레드가 동시에 하나의 객체 및 변수 등 공유 자원에 접근할 때, 의도한 대로 동작하는 것을 말함.
- 공유 자원에 접근하는 임계 영역(Critical Section)을 동기화(Synchronization) 기법으로 "상호 배제"로 실행돼야 함.
- Reentrant(재진입성)해야 함.(여러 Thread가 동시에 접근해도 항상 같은 실행 결과를 보장)
