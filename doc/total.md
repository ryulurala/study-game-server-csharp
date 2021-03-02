---
title: "MMO C# Game-Server"
category: Game-Server
tags:
  [
    c#,
    blocking,
    non-blocking,
    call-back,
    event,
    action,
    func,
    server,
    game-server,
    web-server,
    .net-core,
  ]
date: "2021-02-08"
---

## 개념 정리(시작하기 앞서)

### 동기 vs 비동기

|                                    | 동기 | 비동기 |
| :--------------------------------: | :--: | :----: |
|     요청과 결과가 동시에 발생      |  O   |   X    |
| 함수의 결과가 호출한 함수에서 처리 |  O   |   X    |

### Blocking vs Non-blocking

- Blocking  
  : 내가 일을 하고 있을 때, 내가 제어권을 갖는다.

- Non-blocking  
  : 내가 일을 하고 있을 때, 날 호출한 쪽이 제어권을 갖는다.

### Call-back(콜백 함수)

- 어떤 이벤트가 발생했을 때, 수행해야 하는 함수

#### Example 1: delegate(대리자) 이용

- `=`, `+=`, `-=` 로 참조 가능
  - `=`: `new` 대신 사용
  - `+=` / `-=`: delegate chain `등록` / `취소`

```C#
using System;

namespace DelegateExample
{
    class Program
    {
        delegate void MyDelegate();   // 대리자 정의

        static void Main(string[] args)
        {

            MyDelegate myDelegate;    // 대리자 변수 선언

            myDelegate = A;     // A 함수 등록, 처음에만 '=' 연산자 오버로딩 or new 키워드
            myDelegate += B;    // B 함수 등록
            myDelegate += delegate ()
            {
                Console.WriteLine("Anonymous()");   // 익명 함수 등록
            };
            myDelegate += () =>     // 람다 함수 등록
            {
                Console.WriteLine("Lambda()");
            };

            myDelegate();   // A() -> B() -> Anonymous() -> Lambda() 출력

            // A(), B(), 익명 함수, 람다 함수 모두 콜백 함수로 등록했다.(콜백 체인)
        }

        static void A()
        {
            Console.WriteLine("A()");
        }

        static void B()
        {
            Console.WriteLine("B()");
        }
    }
}
```

#### Example 2: event 이용

- 객체 상태 변화, 이벤트 발생 용도로 사용

- event 는 외부에서 직접 사용이 불가능하다.  
  (static 키워드를 사용하지 않는다면)

- 이벤트 만들기(과정)
  1. delegate 한정자로 델리게이트 선언(클래스 안밖 상관없음)
  2. 델리게이트의 인스턴스를 event 한정자로 선언
  3. 이벤트 핸들러 작성(델리게이트와 일치하는 메소드)
  4. 이벤트 핸들러에 등록(`+=` / `-=`으로 `등록` / `취소` 가능)

```C#
using System;

namespace DelegateExample
{
    class Program
    {
        delegate void MyDelegate();   // 대리자 정의
        static event MyDelegate MyEvent;    // 이벤트 선언

        static void Main(string[] args)
        {
            MyEvent = A;     // A 함수 등록, 처음에만 '=' 연산자 오버로딩
            MyEvent += B;    // B 함수 등록
            MyEvent += delegate ()
            {
                Console.WriteLine("Anonymous()");   // 익명 함수 등록
            };
            MyEvent += () =>     // 람다 함수 등록
            {
                Console.WriteLine("Lambda()");
            };

            MyEvent();   // A() -> B() -> Anonymous() -> Lambda() 출력

            // A(), B(), 익명 함수, 람다 함수 모두 콜백 함수로 등록했다.(콜백 체인)
        }

        static void A()
        {
            Console.WriteLine("A()");
        }

        static void B()
        {
            Console.WriteLine("B()");
        }
    }
}
```

#### Exmaple 3: Action 이용

- Action\<T, T, T, ...>  
  : Action 변수 명: 매개 변수 없음
  : Action\<T1>: 매개 변수 T1  
  : Action\<T1, T2>: 매개 변수 T1, T2  
  : Action\<T1, T2, T3>: 매개 변수 T1, T2, T3

- 무조건 반환 타입은 void 형이다.

- 반환 형이 없이 확실히 없을 때 사용.

```C#
using System;

namespace ActionExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Action myAction;

            myAction = A;     // A 함수 등록, 처음에만 '=' 연산자 오버로딩 or new 키워드
            myAction += B;    // B 함수 등록
            myAction += delegate ()
            {
                Console.WriteLine("Anonymous()");   // 익명 함수 등록
            };
            myAction += () =>     // 람다 함수 등록
            {
                Console.WriteLine("Lambda()");
            };

            myAction();   // A() -> B() -> Anonymous() -> Lambda() 출력

            // A(), B(), 익명 함수, 람다 함수 모두 콜백 함수로 등록했다.(콜백 체인)
        }

        static void A()
        {
            Console.WriteLine("A()");
        }

        static void B()
        {
            Console.WriteLine("B()");
        }
    }
}
```

#### Example 4: Func 이용

- Func\<T, T, T, ..., return type>  
  : Func\<T1>: return T1, 매개 변수 없음  
  : Func\<T1, T2>: 매개 변수 T1 / return T2  
  : Func\<T1, T2, T3>: 매개 변수 T1, T2 / return T3  
  : Func\<T1, T2, T3, T4>: 매개 변수 T1, T2, T3 / return T4

- 반환 타입이 존재해야 함.

- 반환 형이 있고 반환 타입을 알고 있을 때 사용.

```C#
using System;

namespace FuncExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Func<int> myFunc;     // 반환 타입이 있어야 함, <return type>

            myFunc = A;     // A 함수 등록, 처음에만 '=' 연산자 오버로딩 or new 키워드
            myFunc += B;    // B 함수 등록
            myFunc += delegate ()
            {
                Console.WriteLine("Anonymous()");   // 익명 함수 등록
                return 1;       // int 반환
            };
            myFunc += () =>     // 람다 함수 등록
            {
                Console.WriteLine("Lambda()");
                return 1;       // int 반환
            };

            myFunc();   // A() -> B() -> Anonymous() -> Lambda() 출력

            // A(), B(), 익명 함수, 람다 함수 모두 콜백 함수로 등록했다.(콜백 체인)
        }

        static int A()
        {
            Console.WriteLine("A()");
            return 1;       // int 반환
        }

        static int B()
        {
            Console.WriteLine("B()");
            return 1;       // int 반환
        }
    }
}
```

#### 콜백 함수(C# 문법) 정리

|                         |  delegate   |            event            |        Action        |         Func\<T>          |
| :---------------------: | :---------: | :-------------------------: | :------------------: | :-----------------------: |
| delegate 함수 형식 지정 |    필수!    |       delegate에 한정       |          X           |             X             |
|   반환 타입 명시 여부   |    가능     |       delegate에 한정       |  불가능(Only. void)  | 가능(<T, T, return type>) |
|          용도           | type 불명확 | 객체 상태 변화 or 사건 발생 | 반환 type 없음(명확) |   반환 type 있음(명확)    |

---

## 서버 개요

### 서버

- 다른 컴퓨터에서 연결이 가능하도록 대기 상태로 상시 실행 중인 프로그램

### Web Server vs Game Server

|           Web Server            |          Game Server           |
| :-----------------------------: | :----------------------------: |
|            Stateless            |            Stateful            |
|       실시간 상호 작용 X        |       실시간 상호 작용 O       |
| 질의(Query) / 대답(Answer) 형태 | 요청(Request) / 응답(Response) |

### Web Server(aka. HTTP Server)

- 질의(Query) / 대답(Answer) 형태
- 실시간 상호 작용 X
- `Stateless`: 서버가 (현재 클라이언트의)상태를 모름
- 프레임워크 종류
  - `ASP.NET(C#)`
  - `Spring(Java)`
  - `NodeJS(JavaScript)`
  - `Django`
  - `Flask(Python)`
  - `PHP`
  - ...

### Game Server(aka. TCP Server, Binary Server, Stateful Server ...)

- 요청(Request) / 응답(Response) 형태
- 실시간 상호 작용 O
- `Stateful`: 서버가 (현재 클라이언트의)상태를 알고 접근도 가능.
- 게임 장르에 따라 요구 사항이 천차만별.
- 최적의 프레임워크라는 것이 존재하기 힘들다.

### Game Server : 식당

|   게임 서버    |               식당               |
| :------------: | :------------------------------: |
| 동시 접속자 수 |             손님 수              |
|   게임 장르    |             인테리어             |
|    직원 수     |            쓰레드 수             |
|   게임 로직    |              요리사              |
|    네트워크    |            서빙 직원             |
|  데이터베이스  |           장부 및 결제           |
|  쓰레드 모델   | 요리사 / 서빙 / 결제 직원들 비율 |
| 네트워크 모델  |          주문하는 방법           |

---

## (CLI)환경 설정

### 설치

- `VSCode(Visual Studio Code)` 다운로드 및 설치
- `C# Extension(VSCode Extension)` 다운로드 및 설치
- `.NET Core SDK(64bits)` 다운로드 및 설치

### 솔루션 및 프로젝트 생성

- `.sln` 파일: 여러 프로젝트를 빌드 용도
- `.csproj` 파일: 프로젝트에 관한 정보로 프로젝트 실행 가능하도록 함.

1. 솔루션 디렉토리(새 폴더) 생성
2. 솔루션 디렉토리에서 Terminal 실행 및 명령어 입력
   - `dotnet new sln` : 솔루션 파일 생성(여러 프로젝트 빌드)
3. 프로젝트 디렉토리(새 폴더) 생성
4. 프로젝트 디렉토리에서 Terminal 실행 및 명령어 입력
   - `dotnet new console` : 콘솔 기반 프로젝트 생성
5. 솔루션 디렉토리에서 Terminal 실행 및 명령어 입력
   - `dotnet sln add "프로젝트명.csproj"` : 솔루션 파일에 프로젝트 등록
     > dotnet sln add aaa/bbb/ccc/test.csproj
   - 부모 디렉토리에서 `dotnet sln add *` 으로 등록가능
     (대신 등록 불가한 것에 대한 에러 조금 발생)
   - `dotnet sln list` 로 등록 여부 확인

### 실행(CLI)

- 프로젝트 파일로 실행

  - 프로젝트 디렉토리에서 Terminal 실행 및 명령어 입력  
    `dotnet run`

- 솔루션 파일로 실행

  - 솔루션 디렉토리에서 Terminal 실행 및 명령어 입력  
    `dotnet run --project "프로젝트명"`
    > dotnet run --project test

### Example

1. 최상위 디렉토리(`Server-Example`) 생성
2. `Server-Example` 하위 디렉토리로 `Server`, `ServerCore`, `DummyClient` 디렉토리를 생성

   - `Server` : 실질적인 서버 - Main() 실행
   - `ServerCore` : Server가 사용할 API 모음
   - `DummyClient` : 많은 Client 접속 예제 - Main() 실행

3. `dotnet new sln`을 통해서 최상위 디렉토리(Server-Example)에 솔루션 파일(`.sln`) 생성
4. `dotnet sln add *`로 해당 폴더부터 하위 폴더 모두를 검사하여 프로젝트 파일(`.csproj`)를 등록
5. `.sln` 파일 위치에서 `dotnet run --project "프로젝트명"` 입력하여 실행

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

## 컴파일러 최적화

### Debug vs Release

|            Debug            |           Release           |
| :-------------------------: | :-------------------------: |
| 디버깅 정보가 포함돼 느리다 | Debug 모드보다 3~4배 빠르다 |
|        코드 최적화 X        |        코드 최적화 O        |

- **Debug, Release 모드 둘 다 실행되어야 정상적인 프로그램이다.**

### 코드 최적화 예제

```cs
using System;
using System.Threading;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static bool _stop = false;

        static void ThreadMain()
        {
            Console.WriteLine("쓰레드 시작");
            while (!_stop) ;  // stop 신호 기다리기
            Console.WriteLine("쓰레드 종료");
        }
        static void Main(string[] args)
        {
            Task task = new Task(ThreadMain);
            task.Start();

            Thread.Sleep(1000); // 1초를 기다림(1000ms)

            _stop = true;

            Console.WriteLine("종료 대기중");
            task.Wait();    // thread의 Join() 과 동일
            Console.WriteLine("종료 성공");
        }
    }
}
```

### Debug Mode

```cs
// Debug Mode
while(!_stop)
{
  // stop 신호 기다리기
  // _stop=true 하는 code가 없으므로 Release Mode에서는 코드 최적화가 일어남.
}
```

### Release Mode

```cs
if(!_stop)
{
  while(true)
  {
    // stop 신호 기다리기
  }
}
```

### 코드 최적화 방지

- `volatile` 키워드 사용
  > 가시성 확보  
  > `static 타입 변수명;` => `volatile static 타입 변수명;`  
  > volatile은 휘발성 데이터로 코드 작성한 그대로 쓰게 한다.  
  > `C#`과 `C++`에서의 `volatile` 키워드는 다르다.  
  > `C++`에서의 `volatile` 키워드는 최신 값을 가져오는 역할

---

## 캐시 이론

- CPU 코어 <-> RAM(메인 메모리)

  > CPU 코어: ALU(연산 장치) + Cache(캐시 장치)  
  > Single-Thread 환경에서는 문제가 없다.  
  > Multi-Thread 환경에서는 각 Thread마다의 정보가 다르다 .

### Locality(지역성)

1. `Tempolal Locality` : 시간 - 최근 접근한 것이 또 접근할 확률이 높다.
2. `Spacial Locality` : 공간 - 접근한 것의 근처에 접근할 확률이 높다.

### `Spacial Locality` 예제

```cs
using System;
using System.Threading;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            int[,] arr = new int[10000, 10000]; // 2차원 배열
            {
                long start = DateTime.Now.Ticks;  // 1ms=10,000 Ticks
                for (int y = 0; y < 10000; y++)
                {
                    for (int x = 0; x < 10000; x++)
                    {
                        arr[y, x] = 1;  // 2차원 배열 접근(1개씩 순차적으로)
                    }
                }
                long end = DateTime.Now.Ticks;
                Console.WriteLine($"(y, x) 걸린 시간${end - start}");   // 3297570
            }
            {
                long start = DateTime.Now.Ticks;
                for (int y = 0; y < 10000; y++)
                {
                    for (int x = 0; x < 10000; x++)
                    {
                        arr[x, y] = 1;  // 2차원 배열 접근(10000개씩 건너뛰면서)
                    }
                }
                long end = DateTime.Now.Ticks;
                Console.WriteLine($"(y, x) 걸린 시간${end - start}");   // 4962996
            }
        }
    }
}
```

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

## Interlocked

- `Interlocked`
  > `Race Condition`(경합 조건)에서 공유 변수의 값을 `Atomic`(원자적)하게 바꿔준다.  
  > 공유 변수에 대해 순서를 보장한다.  
  > `System.Threading` 클래스에 존재.  
  > `Memory Barrier`를 간접적으로 사용.

### `Interlocked` 예제

- 잘못된 코드

  > 공유 변수 `number` 값이 `0`이 나오지 않는 경우가 많다.

  ```cs
  using System;
  using System.Threading;
  using System.Threading.Tasks;

  namespace test
  {
      class Program
      {
          static volatile int number = 0;
          static void Thread_1()
          {
              for (int i = 0; i < 100000; i++)
                  number++;
          }

          static void Thread_2()
          {
              for (int i = 0; i < 100000; i++)
                  number--;
          }

          static void Main(string[] args)
          {
              Task task1 = new Task(Thread_1);
              Task task2 = new Task(Thread_2);
              task1.Start();
              task2.Start();

              Task.WaitAll(task1, task2);

              Console.WriteLine($"number={number}");  // 0이 나오지 않을 확률이 높음
          }
      }
  }
  ```

- 올바른 코드

  > `Interlocked` 클래스를 활용하여 `Atomic` 하게 값을 변경한다.

  ```cs
  using System;
  using System.Threading;
  using System.Threading.Tasks;

  namespace test
  {
      class Program
      {
          static int number = 0;
          static void Thread_1()
          {
              for (int i = 0; i < 100000; i++)
                  Interlocked.Increment(ref number);  // number++; 과 같다.
          }

          static void Thread_2()
          {
              for (int i = 0; i < 100000; i++)
                  Interlocked.Decrement(ref number);  // number--; 와 같다.
          }

          static void Main(string[] args)
          {
              Task task1 = new Task(Thread_1);
              Task task2 = new Task(Thread_2);
              task1.Start();
              task2.Start();

              Task.WaitAll(task1, task2);

              Console.WriteLine($"number={number}");
          }
      }
  }
  ```

- 프로그래머가 작성한 코드

  ```cs
  number++;   // 값을 증가
  ```

- 컴파일러 최적화로 인한 코드

  ```cs
  int temp=number;    // 값을 복사
  temp +=1; // 복사한 값을 증가
  number = temp;    // 증가한 값을 저장
  ```

- `Atomic`하게 변경한 코드

  ```cs
  Interlocked.Increment(ref number);  // number++과 같다(Atomic 하게 이루어짐)
  // ref: 주소값을 매개변수로 사용, 복사값 X
  ```

### `Interlocked` 클래스

|           |                  Increment                   |                  Decrement                   |            Exchange             |                             CompareExchange                              |
| :-------: | :------------------------------------------: | :------------------------------------------: | :-----------------------------: | :----------------------------------------------------------------------: |
| 매개변수  |                 ref location                 |                 ref location                 |    ref location, type value     |                 ref location type value, type comparand                  |
| return 값 |               location 결과값                |               location 결과값                |         location 결과값         |                             location 결과값                              |
|   설명    | location 값을 1증가 후 저장(only. int, long) | location 값을 1감소 후 저장(only. int, long) | location 값을 value 값으로 바꿈 | location과 comparand를 비교 후 값이 같다면, location을 value 값으로 저장 |

---

## Lock 기초

### `Interlocked`

- 정수형 증가, 감소
- 값 비교 및 바꾸기

```cs
int number = 0;
Interlocked.Increment(ref number);  // number++; 과 동일.
```

### `Monitor`

- `Interlocked`보다 복잡한 일 가능.
- `Enter`와 `Exit` 구간에서는 싱글 쓰레드 환경처럼 `Atomic`하게 사용 가능.
- `DeadLock` 발생 가능

```cs
int number = 0;
object _obj = new object();
try
{
    Monitor.Enter(_obj);  // 잠금
    number++;
}
finally
{
    Monitor.Exit(_obj); // 잠금 해제, 무조건 Enter()를 하면 Exit()을 해줘야 함.
}
```

### `lock(objcet obj)`

- 내부적으로 `Monitor`로 실행된다.
- `DeadLock` 발생 가능

```cs
int number = 0;
object _obj = new object();

lock(_obj)  // 잠금
{
    number++;
} // 빠져나올 때 알아서 잠금 해제
```

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

## SpinLock

- 무작정 기다리기

### SpinLock 예제

- 잘못된 방법

  > 동시에 들어갈 수 있는 여지가 있다.

  ```cs
  class SpinLock
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
  class SpinLock
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

## ReaderWriterLock

- Write를 할 경우는 상호-배타적으로 실행해야한다.
  - `Wirte`---상호-배타 O---`Write`
  - `Write`---상호-배타 O---`Read`
  - `Read`---상호-배타 X---`Read`
- 공유 자원을 조회할 경우와 수정할 경우의 lock 비율이 많이 차이날 경우 사용.
- In Game
  - 게임 아이템은 최종적으로는 DB에 저장되지만 DB부하와 다른 이유로 DB에서 가져와 서버 메모리에서 처리해야 할 경우가 존재.
  - Example: 특정 던전을 유도하기 위해 특정 시간에 운영자가 운영툴로 게임 보상을 추가
    > 보상이 5개에서 6개로 오를 경우, Lock을 걸지 않고는 Data에 수정하기는 어렵다.  
    > 이 경우, User들은 Read할 경우는 User의 수만큼이지만 Write는 단 한 번이다.  
    > 여기서 Reader의 Lock을 Writer의 Lock과 같은 수준을 걸게 되면 매우 낭비다.

### ReaderWriterLock API

- ReaderWriterLock or ReaderWriterLockSlim 객체
  > ReaderWriterLockSlim이 더 최신(**권장**)

```cs
class Reward { }
class RewardManager
{
    ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

    Reward getRewardById(int id)    // 99.9999 %
    {
        rwLock.EnterReadLock();

        // Reward 읽기(= Read)
        Reward reward = new Reward();

        rwLock.ExitReadLock();

        return reward;
    }

    void addReward()    // 0.0001 %
    {
        rwLock.EnterWriteLock();

        // Reward 추가(= Write)

        rwLock.ExitWriteLock();
    }
}
```

### ReaderWriterLock 직접 구현

#### ReaderWriterLock 구조

1. 정책 결정
   > 재귀적 락 허용 O: `WriteLock`에 대해서 `WriteLockCount`를 매길 건지.  
   > 재귀적 락 허용 X: `WriteLock`을 잡은 상태에서 `WriteLock` or `ReadLock`을 못 잡음
2. 스핀락 정책

   > 몇 번의 시도 끝에 양보할 건지

3. Bit Masking

   > 원자성을 강조한다.

   - `[Unused(1)]`: 음수가 되는 것을 방지
   - `[WriteThreadId(15)]`: Write 한 번에 한 Thread만 Lock을 잡을 때, 해당 `Thread ID`
   - `[ReadCount(16)]`: 여러 Thread들이 동시에 잡을 때, Thread 개수

   ```cs
   // bit flag
   const int EMPTY_FLAG = 0x00000000;
   const int WRITE_MASK = 0x7FFF0000;  // & 연산으로 추출
   const int READ_MASK = 0x0000FFFF; // & 연산으로 추출
   const int MAX_SPIN_COUNT = 5000;  // 몇 번 시도하다 양보할 건지

   // [Unused(1)] [WriteThreadid(15)] [ReadCount(16)]
   int _flag = EMPTY_FLAG;
   ```

4. InterLock 계열 사용

   > `Atomic`하게 사용해야 한다.  
   > `CaS(Compare & Swap)`연산을 사용.

   ```cs
   // 잘못된 코드 예시
   if(current==expected) // 현재 값이 예상 값일 때(1단계)
   {
       current=desired;  // 현재 값을 원하는 값으로 바꾸기(2단계)
       return;
   }

   // 올바른 코드 예시
   if (Interlocked.CompareExchange(ref _flag, desired, expected) == expected)  // 1단계 끝
       return;
   ```

5. `WriteLock()`, `WriteUnlock()`, `ReadLock()`, `ReadUnlock()`

   ```cs
   public void WriteLock(){ }  // 아무도 WriteLock과 ReadLock을 소유 X 일 때, 경합해서 소유권 획득
   public void WriteUnlock(){ }  // WriteLock을 초기 상태로
   public void ReadLock(){ }   // 아무도 WriteLock을 소유 X 일 때, ReadCount를 1 증가
   public void ReadUnlock(){ }   //  ReadCount를 1 감소
   ```

#### 재귀적 허용 X 코드

```cs
// bit flag
const int EMPTY_FLAG = 0x00000000;
const int WRITE_MASK = 0x7FFF0000;  // & 연산으로 추출
const int READ_MASK = 0x0000FFFF; // & 연산으로 추출
const int MAX_SPIN_COUNT = 5000;  // 몇 번 시도하다 양보할 건지

// [Unused(1)] [WriteThreadid(15)] [ReadCount(16)]
int _flag = EMPTY_FLAG;

public void WirteLock()
{
    // 아무도 WriteLock or ReadLock을 소유 X 일 때, 경합해서 소유권 획득
    int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;    // current Thread id
    while (true)
    {
        for (int i = 0; i < MAX_SPIN_COUNT; i++)
        {
            // 성공하면 return
            if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
                return;
        }

        Thread.Yield();     // 양보
    }
}

public void WirteUnlock()
{
    // 초기 상태로
    Interlocked.Exchange(ref _flag, EMPTY_FLAG);
}

public void ReadLock()
{
    // 아무도 WriteLock을 획득 X 일 때, ReadCount++;
    while (true)
    {
        for (int i = 0; i < MAX_SPIN_COUNT; i++)
        {
            int expected = _flag & READ_MASK;
            // 현재 쓰레드가 이전 쓰레드의 expected 값을 바꿈
            if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                return;
        }

        Thread.Yield(); // 양보
    }
}

public void ReadUnlock()
{
    // ReadCount--;
    Interlocked.Decrement(ref _flag);
}
```

#### 재귀적 허용 O 코드

```cs
// bit flag
const int EMPTY_FLAG = 0x00000000;
const int WRITE_MASK = 0x7FFF0000;  // & 연산으로 추출
const int READ_MASK = 0x0000FFFF; // & 연산으로 추출
const int MAX_SPIN_COUNT = 5000;

// [Unused(1)] [WriteThreadid(15)] [ReadCount(16)]
int _flag = EMPTY_FLAG;
int _writeCount = 0;    // 재귀적 락 허용 시, count

public void WirteLock()
{
    // 동일 Thread가 WirteLock을 이미 획득했는지 확인
    int lockThreadId = (_flag & WRITE_MASK) >> 16;
    if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
    {
        _writeCount++;  // 이미 WriteLock은 상호-배타적임, ReadLock은 상호-배타적 X
        return;
    }

    // 아무도 WriteLock or ReadLock을 소유 X 일 때, 경합해서 소유권 획득
    int desired = (Thread.CurrentThread.ManagedThreadId << 16) & WRITE_MASK;    // current Thread id
    while (true)
    {
        for (int i = 0; i < MAX_SPIN_COUNT; i++)
        {
            // 성공하면 return
            if (Interlocked.CompareExchange(ref _flag, desired, EMPTY_FLAG) == EMPTY_FLAG)
            {
                _writeCount = 1;
                return;
            }
        }

        Thread.Yield();     // 양보
    }
}

public void WirteUnlock()
{
    // WriteLock이 0이 됐을 때만 초기 상태로
    int lockCount = --_writeCount;
    if (lockCount == 0)
        Interlocked.Exchange(ref _flag, EMPTY_FLAG);    // 초기 상태로
}

public void ReadLock()
{
    // 동일 Thread가 WirteLock을 이미 획득했는지 확인
    int lockThreadId = (_flag & WRITE_MASK) >> 16;
    if (Thread.CurrentThread.ManagedThreadId == lockThreadId)
    {
        Interlocked.Increment(ref _flag);   // ReadCount++;
        return;
    }

    // 아무도 WriteLock을 획득 X 일 때, ReadCount++;
    while (true)
    {
        for (int i = 0; i < MAX_SPIN_COUNT; i++)
        {
            int expected = _flag & READ_MASK;
            // 현재 쓰레드가 이전 쓰레드의 expected 값을 바꾸지 않으면
            if (Interlocked.CompareExchange(ref _flag, expected + 1, expected) == expected)
                return;
        }

        Thread.Yield(); // 양보
    }
}

public void ReadUnlock()
{
    // ReadCount--;
    Interlocked.Decrement(ref _flag);
}
```

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

## 네트워크 이론

### 인터넷 통신 구조 vs 택배 배송 구조

|      인터넷 통신       |      택배 배송       |
| :--------------------: | :------------------: |
|          패킷          |      택배 상품       |
|         라우터         |      배송 업체       |
|        네트워크        |     아파트 단지      |
|         스위치         |    단지 내 경비실    |
|         컴퓨터         |          집          |
| 사설 주소(Private, IP) | ex) 김씨 집, 최씨 집 |
| 공식 주소(Public, IP)  |   ex) 201호, 502호   |

### 통신 모델

#### OSI-7 Layer vs TCP/IP 5-Layer

- 단계가 증가될 때마다 패킷을 이어붙인다.

| 계층 |            OSI-7 모델             |                TCP/IP 모델                 | 택배 배송 |
| :--: | :-------------------------------: | :----------------------------------------: | :-------: |
|  7   | **Application**(`Domain address`) |                                            |           |
|  6   |         **Presentation**          |                                            |           |
|  5   |            **Session**            | **Application**(`HTTP`, `FTP`, `DNS`, ...) | 택배 상품 |
|  4   |           **Transport**           |        **Transport**(`TCP`, `UDP`)         | 배송 정책 |
|  3   |     **Network**(`IP address`)     |        **Network**(`IP`, `Router`)         | 최종 주소 |
|  2   |   **Data Link**(`MAC address`)    |   **Data Link**(`Switch`, `Ethernet`, )    | 단지 경로 |
|  1   |           **Physical**            |      **Physical**(`신호 처리`, `Hub`)      | 택배 운송 |

#### Address

|               Domain address                |                   IP address                    |                       MAC address                       |
| :-----------------------------------------: | :---------------------------------------------: | :-----------------------------------------------------: |
|          해당 IP에 대한 별명 주소           |   해당 컴퓨터(Host)로 찾아가기 위한 식별 주소   | H/W 기기 자체에 부여된 고유한 식별 번호(세상에 단 하나) |
|         TLD: `.com`, `.abc`, `.io`          | Private IP: 공유기로 할당된 주소, `192.168.x.x` |            물리적 주소: `xx-xx-xx-xx-xx-xx`             |
| etc: `x.x`(.com), `x.x`(.abc), `x.x.x`(.io) |    Public IP: 외부에서 보이는 IP, `x.x.x.x`     |                                                         |

#### TCP, UDP

|               TCP                |            UDP             |
| :------------------------------: | :------------------------: |
|           신뢰성 중요            |     속도(연속성) 중요      |
|        연결 지향(Socket)         |   비연결 지향(Only. IP)    |
|           순서 보장[O]           |        순서 보장[X]        |
|           Segment 패킷           |       Datagram 패킷        |
| `HTTP`, `Email`, `File transfer` |           `DNS`            |
|    흐름 제어[O], 혼잡 제어[O]    | 흐름 제어[X], 혼잡 제어[X] |
|  전송량과 전송 속도를 제어한다.  |        그런거 없음         |

#### Router vs Switch vs Hub

- `기기`---`허브`---`스위치`---`라우터`---`인터넷`---`라우터`---`스위치`---`허브`---`기기`

|                      Router                       |                     Switch                     |                             Hub                              |
| :-----------------------------------------------: | :--------------------------------------------: | :----------------------------------------------------------: |
| 통신망(Network)끼리 전송으로 최종 통신망으로 전송 | 하나의 통신망 안에서 Port별로 최종 목적지 전송 | 신호 증폭 + 거리 연장, 연결된 장치 모두 전송 용도(Broadcast) |
|          IP 주소를 사용(`Network Layer`)          | MAC 주소, Port 번호를 사용(`Data Link Layer`)  |              전기적 신호 사용(`Physical Layer`)              |

#### Internet vs Ethernet

|                  Internet                  |               Ethernet               |
| :----------------------------------------: | :----------------------------------: |
| 전 세계의 네트워크 묶음을 하나로 묶은 기술 | LAN(근거리 통신망)에서 사용하는 기술 |

#### Protocol

|                        HTTP                        |                              HTTPS                              |      FTP       |       SMTP       |
| :------------------------------------------------: | :-------------------------------------------------------------: | :------------: | :--------------: |
| 인터넷 상에서 문서(HyperText)를 전달하기 위한 규약 | HTTP 규약 + Secure Socket, 보안 요소 추가된 HyperText 통신 규약 | 파일 전송 규약 | 이메일 전송 규약 |

---

## Socket Programming

### Store vs Server/Client

- |  식당 주인  |              Server               |
  | :---------: | :-------------------------------: |
  | 문지기 고용 |            `Socket()`             |
  | 문지기 교육 | `Bind()`: `IP` + `Port` -> Socket |
  |  영업 시작  |            `Listen()`             |
  |    안내     |            `Accept()`             |

- |         손님          |  Client   |
  | :-------------------: | :-------: |
  |      휴대폰 준비      | Socket()  |
  | 식당 번호로 입장 문의 | Connect() |
  |        대리인         |  Session  |

### Socket API(`C#`)

#### Server-Socket

1. `IP Address`

   > `DNS`(`Domain Name System`)를 이용.

   ```cs
   // Localhost
   string host = Dns.GetHostName();

   // Get: IP 주소 배열
   // GetHostEntry(String hostName)
   IPHostEntry ipHost = Dns.GetHostEntry(host);

   // 분산 서버 중 첫 번째
   IPAddress ipAddr = ipHost.AddressList[0];
   ```

2. `End Point`

   > `IP Address` + `Port`

   ```cs
   // IPEndpoint(IP address, Port)
   IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
   ```

3. `Socket()`

   > 소켓 생성  
   > = 문지기 고용

   ```cs
   // Socket(AddressFamily, SocketType, ProtocolType)
   Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
   ```

4. `Bind()`

   > 소켓 연동  
   > = 문지기 교육

   ```cs
   // Bind(EndPoint)
   listenSocket.Bind(endPoint);
   ```

5. `Listen()`

   > = 영업 시작

   - backlog
     > 동시 접속자에 의한 최대 대기 수  
     > 몇 명이 대기할 지...  
     > backlog를 초과하면 접속자 모두 fail

   ```cs
   // Listen(int backlog)
   // Accept() 되기 전까지 10명 대기
   listenSocket.Listen(10);
   ```

6. `Accept()`

   > 접속 허용  
   > = 손님 안내  
   > Client Socket을 반환  
   > Blocking 함수: 입장할 때까지 무한정 대기

   ```cs
   // Socket Accept()
   Socket clientSocket = listenSocket.Accept();
   ```

7. `Send()` / `Receive()`

   - `Receive()`
     > 패킷 수신  
     > `Overload`된 함수에 따라 반환 값이 다름  
     > Blocking 함수

   ```cs
   // 수신 버퍼
   byte[] recvBuff = new byte[1024];

   // int Receive(byte[] buffer)
   int recvBytes = clientSocket.Receive(recvBuff);

   // byte buffer 디코딩, 규약: UTF-8
   // GetString(buffer, start-index, byte-size)
   string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
   ```

   - `Send()`
     > 패킷 송신  
     > `Overload`된 함수에 따라 반환 값이 다름  
     > BLocking 함수

   ```cs
   // 송신 버퍼: byte buffer 인코딩, 규약: UTF-8
   byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server !");

   // int Send(byte[] buffer)
   int sendBytes = clientSocket.Send(sendBuff);
   ```

8. `Close()`

   > Client 연결 끊기  
   > = 손님 보내기

   ```cs
   // Shutdown(SocketShutdown how): 예고, 신뢰성(TCP)
   clientSocket.Shutdown(SocketShutdown.Both);

   // Close()
   clientSocket.Close();
   ```

#### Client-Socket

1. `IP Address`

   > `DNS`(`Domain Name System`)를 이용.

   ```cs
   // Localhost
   string host = Dns.GetHostName();

   // Get: IP 주소 배열
   // GetHostEntry(String hostName)
   IPHostEntry ipHost = Dns.GetHostEntry(host);

   // 분산 서버 중 첫 번째
   IPAddress ipAddr = ipHost.AddressList[0];
   ```

2. `End Point`

   > `IP Address` + `Port`

   ```cs
   // IPEndpoint(IP address, Port)
   IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);
   ```

3. `Socket()`

   > 소켓 생성  
   > = 휴대폰 설정

   ```cs
   // Socket(AddressFamily, SocketType, ProtocolType)
   Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
   ```

4. `Connect()`

   > 접속 요청  
   > = 식당 입장 문의
   > Blocking 함수: 접속 성공까지 무한정 대기, **게임에서 치명적**

   ```cs
   // Connect(EndPoint)
   socket.Connect(endPoint);

   // 연결된 위치
   Console.WriteLine($"Connected to {socket.RemoteEndPoint.ToString()}");
   ```

5. `Send()` / `Receive()`

   - `Send()`
     > 패킷 송신  
     > `Overload`된 함수에 따라 반환 값이 다름  
     > BLocking 함수: 서버 측에서 받을 때까지 무한정 대기, **게임에서 치명적**

   ```cs
   // 송신 버퍼: byte buffer 인코딩, 규약: UTF-8
   byte[] sendBuff = Encoding.UTF8.GetBytes("Hello MMOServer !");

   // int Send(byte[] buffer)
   int sendBytes = socket.Send(sendBuff);
   ```

   - `Receive()`
     > 패킷 수신  
     > `Overload`된 함수에 따라 반환 값이 다름  
     > Blocking 함수: 서버 측에서 올 때까지 무한정 대기, **게임에서 치명적**

   ```cs
   // 수신 버퍼
   byte[] recvBuff = new byte[1024];

   // int Receive(byte[] buffer)
   int recvBytes = socket.Receive(recvBuff);

   // byte buffer 디코딩, 규약: UTF-8
   // GetString(buffer, start-index, byte-size)
   string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
   ```

6. `Close()`

   > 연결 끊기  
   > = 퇴장

   ```cs
   // Shutdown(SocketShutdown how): 예고, 신뢰성(TCP)
   clientSocket.Shutdown(SocketShutdown.Both);

   // Close()
   clientSocket.Close();
   ```

#### Server-Client Socket test

- ![socket-api](/uploads/socket/socket-api.gif)

- Server

  ```cs
  static void Main(string[] args)
  {
      // IP 주소
      string host = Dns.GetHostName();
      IPHostEntry ipHost = Dns.GetHostEntry(host);
      IPAddress ipAddr = ipHost.AddressList[0];

      // Port 번호
      IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

      // 문지기(TCP): Listen Socket 초기화
      Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);


      try
      {
          // 문지기 교육: Bind
          listenSocket.Bind(endPoint);

          // 영업시작: Listen
          // backlog: 최대 대기수, 몇 명이 대기할 지... backlog를 초과하면 모두 fail
          listenSocket.Listen(10);    // Accept() 되기 전까지 10명 대기

          while (true)
          {
              Console.WriteLine("Listening...");

              // 손님 안내: Accept()
              Socket clientSocket = listenSocket.Accept();    // Blocking: 다음 단계 불가

              // 메시지 받기: Receive()
              byte[] recvBuff = new byte[1024];
              int recvBytes = clientSocket.Receive(recvBuff);
              string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
              Console.WriteLine($"[From Client] {recvData}");

              // 메시지 보내기: Send()
              byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server !");
              clientSocket.Send(sendBuff);    // Blocking: 다음 단계 불가

              // 손님 보내기: Close()
              clientSocket.Shutdown(SocketShutdown.Both);   // 신뢰성(TCP)
              clientSocket.Close();
          }
      }
      catch (Exception e)
      {
          Console.WriteLine(e);
      }
  }
  ```

- Client

  ```cs
  static void Main(string[] args)
  {
      // IP 주소
      string host = Dns.GetHostName();
      IPHostEntry ipHost = Dns.GetHostEntry(host);
      IPAddress ipAddr = ipHost.AddressList[0];

      // Port 번호
      IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

      // 휴대폰 설정: Socket()

      while (true)
      {
          Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
          try
          {
              // 문지기한테 입장 문의: Connect()
              socket.Connect(endPoint);
              Console.WriteLine($"Connected to {socket.RemoteEndPoint.ToString()}");

              // 메시지 보내기: Send()
              byte[] sendBuff = Encoding.UTF8.GetBytes("Hello MMOServer !");
              int sendBytes = socket.Send(sendBuff);

              // 메시지 받기: Receive()
              byte[] recvBuff = new byte[1024];
              int recvBytes = socket.Receive(recvBuff);
              string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);
              Console.WriteLine($"[From Server] {recvData}");

              // 퇴장: Close()
              socket.Shutdown(SocketShutdown.Both);   // 신뢰성(TCP)
              socket.Close();
          }
          catch (Exception e)
          {
              Console.WriteLine(e);
          }
          Thread.Sleep(500);
      }
  ```

---

## Listener

### Project Library 연결

- `core.csproj`

  > `Library` 로 사용할 Project  
  > `OutputType`을 `Library`로 수정한다.

  ```xml
  <PropertyGroup>
    <OutputType>Library</OutputType>
    <TargetFramework>netcoreapp3.1</TargetFramework>
  </PropertyGroup>
  ```

- `test.csproj`

  > 실제 실행할 `exe` Project  
  > `ProjectReference`(`Library`) 를 `Include` 해준다.

  ```xml
  <ItemGroup>
    <ProjectReference Include="../core/core.csproj"/>
  </ItemGroup>
  ```

### AsyncListener

- Process

  1. init()
     - `new Socket()`
     - `bind()`
     - `Accept()` 결과 콜백 이벤트 등록
     - `listen()`
  2. Accept() 예약
     - `AcceptAsync()`를 이용한 `Pending` 여부
  3. Accept() 완료
     - `init()`으로 등록한 콜백 이벤트 `Invoke`
     - `2.` 다시 실행

#### Listener Code 1

```cs
public class Listener
{
    Socket _listenSocket;   // 서버의 소켓
    Action<Socket> _onAcceptHandler;  // Event Action

    public void Listen(IPEndPoint endPoint, Action<Socket> onAcceptHandler)
    {
        // 서버 소켓을 new, bind, event 연결
        _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // Accept 완료 후 실행될 콜백 Event 연결
        _onAcceptHandler += onAcceptHandler;

        // Bind(), Listen()
        _listenSocket.Bind(endPoint);
        _listenSocket.Listen(10);

        // 이벤트 방식으로 비동기 구현(콜백으로 전달)
        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.Completed += new EventHandler<SocketAsyncEventArgs>  (OnAcceptCompleted);  // 연결됐을 때, 실행할 이벤트 핸들러 등록
        RegisterAccept(args);   // Accept() 예약

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // 기존에 있던 클라이언트 소켓 clear
            args.AcceptSocket = null;

            // Accept()를 요청/등록
            bool pending = _listenSocket.AcceptAsync(args);

            if (!pending)
            {
                //보류 없이 접속됨.
                OnAcceptCompleted(null, args);
            }

            // 접속 보류됨.
            // 서버가 return 시킴.
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs argss)
        {
            // 콜백으로 실행
            if (args.SocketError == SocketError.Success)
            {
                // Accept 성공
                // 이벤트 Invoke
                _onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
            {
                // Accept 실패
                Console.WriteLine(args.SocketError.ToString());
            }
            // 다음 턴을 위한 예약
            RegisterAccept(args);
        }
    }
}
```

#### Listener Code2

- 개선된 코드
- `session`을 상속 받은 여러 `session`들을 생성을 Invoke 한다.

1. Listener Socket init
2. 생성할 Session Factory 콜백 이벤트 연결
   - 추후 Accept() 성공 시 Invoke()
3. Accept() 예약
4. Accept() 성공 시 OnConnected() 콜백 실행

```cs
public class Listener
{
    Socket _listenSocket;   // 서버의 소켓
    Func<Session> _sessionFactory;  // Session을 생성하고 반환

    public void Listen(IPEndPoint endPoint, Func<Session> sessionFactory)
    {
        // 서버 소켓을 new, bind, event 연결
        _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // session을 생성 후 반환하는 콜백 이벤트 연결
        _sessionFactory += sessionFactory;

        // Bind(), Listen()
        _listenSocket.Bind(endPoint);
        _listenSocket.Listen(10);

        // 이벤트 방식으로 비동기 구현(콜백으로 전달)
        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
        RegisterAccept(args);   // Accept() 이벤트 등록
    }
    void RegisterAccept(SocketAsyncEventArgs args)
    {
        // 기존에 있던 클라이언트 소켓 clear
        args.AcceptSocket = null;

        // Accept()를 요청/등록
        bool pending = _listenSocket.AcceptAsync(args);

        if (!pending)
        {
            // 보류 없이 접속됨.
            OnAcceptCompleted(null, args);
        }

        // 접속 보류됨.
    }
    void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
    {
        // 콜백으로 실행
        if (args.SocketError == SocketError.Success)
        {
            // Session을 생성하는 콜백 이벤트를 Invoke()
            Session session = _sessionFactory.Invoke();

            // 해당 session init
            session.init(args.AcceptSocket);

            // 연결되고 실행될 콜백 함수 실행
            session.OnConnected(args.AcceptSocket.RemoteEndPoint);
        }
        else
        {
            // Accept 실패
            Console.WriteLine(args.SocketError.ToString());
        }

        // 다음 턴을 위한 등록
        RegisterAccept(args);
    }
}
```

---

## Session

- `init()`

  > 비동기 완료 콜백 등록  
  > 받을 데이터 버퍼 지정  
  > RegisterReceive()

- `Send()`
  > 한꺼번에 모아서 Send()
  - `Public Send()`
    > 보낼 메시지를 Queue에 계속 Enqueue()  
    > 더 이상 보류된 것이 없을 때 RegisterSend()
  - `RegisterSend()`
    > 보낼 메시지 큐를 비워 한꺼번에 Send() 예약  
    > 비동기 Send() 이므로 예약만 하고 리턴  
    > 만약, 보류 없으면 OnCompletedSend() 호출
  - `OnCompletedSend()`
    > 실질적으로 Send()가 완료됨.  
    > 만약, 보낼 메시지가 큐에 남아있으면 다시 RegisterSend()
- `Receive()`
  - `RegisterRecv()`
    > ReceiveAsync()  
    > 만약 보류 없이 바로 보내지면 OnRecvCompleted
  - `OnCompletedRecv()`
    > 메시지를 다 받음.  
    > 다음 턴을 위해 다시 RegisterRecv()
- `Disconnect()`

  > 두 번 이상 Disconnect() 는 오류이므로, 다른 Thread가 동시에 접근해  
  > Disconnect()를 두 번하는 것을 방지하기 위해 Compare and Swap 이용

- `abstract()`
  > 다른 Session이 상속받아 구현할 함수
  - `OnConnected()`
    > 연결됐을 때 콜백 실행
  - `OnRecv()`
    > 메시지 수신될 떄, 콜백 실행
  - `OnSend()`
    > 메시지 송신 완료할 때, 콜백 실행
  - `OnDisconnected()`
    > 연결이 끊겼을 때 콜백 실행

```cs
abstract public class Session
{
    Socket _socket;     // 클라이언트 소켓(대리자)
    int _disconnected = 0;
    Queue<byte[]> _sendQueue = new Queue<byte[]>();   // 보낼 메시지를 모아서 보냄
    SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();    // 재사용
    SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();    // 재사용
    List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();
    object _lock = new object();

    // 다른 session이 구현할 함수
    public abstract void OnConnected(EndPoint endPoint);
    public abstract void OnRecv(ArraySegment<byte> buffer);
    public abstract void OnSend(int numOfBytes);
    public abstract void OnDisconnected(EndPoint endPoint);

    public void Start(Socket socket)
    {
        _socket = socket;

        // 비동기 완료 콜백 등록
        _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
        _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

        // argument buffer 설정
        // SetBuffer(buffer, offset, count)
        _recvArgs.SetBuffer(new byte[1024], 0, 1024);

        // 등록(예약)
        ResgisterRecv();
    }
    public void Send(byte[] sendBuff)
    {
        lock (_lock)
        {
            // 쓰레드 하나씩
            _sendQueue.Enqueue(sendBuff);

            if (_pendingList.Count == 0) RegisterSend();
        }
    }
    public void Disconnect()
    {
        // Disconnect를 두 번 하면 Error 발생하므로
        // 멀티쓰레드 환경에서는 이미 Disconnect 한 쓰레드는 접근 X
        // 이미 Disconnect 한 쓰레드가 1을 만드는 것 방지.
        // original value가 1일 경우 close() 불가
        if (Interlocked.Exchange(ref _disconnected, 1) == 1) return;

        OnDisconnected(_socket.RemoteEndPoint);

        // 손님 보내기: Close()
        _socket.Shutdown(SocketShutdown.Both);   // 신뢰성(TCP)
        _socket.Close();
    }

    #region 네트워크 통신
    // 내부적이므로 region 설정
    void RegisterSend()
    {
        // Send()에서 lock을 걸기 때문에 따로 lock 걸 필요 X
        while (_sendQueue.Count > 0)
        {
            byte[] buff = _sendQueue.Dequeue();

            // ArraySegment는 C#에서 구조체로, Stack을 이용 - 효율적
            _pendingList.Add(new ArraySegment<byte>(buff, 0, buff.Length));
        }

        // 예약된 목록들
        _sendArgs.BufferList = _pendingList;

        // 여러 번 SendAsync() 호출 시에 부하가 심하다.
        // 예약 확인하며 _sendArgs가 이벤트로 발생시켜 실행한다.
        bool pending = _socket.SendAsync(_sendArgs);
        if (pending == false)
        {
            OnSendCompleted(null, _sendArgs);
        }
    }
    void OnSendCompleted(object sender, SocketAsyncEventArgs args)
    {
        // init() 에서 들어오는 쓰레드가 있으니 Lock을 건다.
        lock (_lock)
        {
            // 상대방이 연결을 끊으면 가끔 0 byte로 온다.
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    // 다른 쓰레드 Enqueue 했던 것을 처리
                    // pendingList를 가지고 있을 필요 X
                    _sendArgs.BufferList = null;
                    _pendingList.Clear();
                    OnSend(_sendArgs.BytesTransferred);

                    if (_sendQueue.Count > 0)
                    {
                        // Queue에 남아있으면 그 쓰레드가 Enqueue한 것을 내가 처리
                        RegisterSend();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnSendCompleted Failed {e.ToString()}");
                }
            }
            else
            {
                // TODO Disconnect
                Disconnect();
            }
        }
    }
    void ResgisterRecv()
    {
        bool pending = _socket.ReceiveAsync(_recvArgs);
        if (!pending)
        {
            // 보류 없이 Receive() 성공
            OnRecvCompleted(null, _recvArgs);
        }
    }
    void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
    {
        if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
        {
            // 성공적으로 Data를 가져옴
            // 상대방이 연결을 끊으면 가끔 0 byte로 옴.
            try
            {
                // (buffer, offset=시작 위치, 받은 byte 수)
                OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));

                // 다시 받을 준비
                ResgisterRecv();
            }
            catch (Exception e)
            {
                // 에러 처리
                Console.WriteLine($"OnRecvCompleted Failed {e.ToString()}");
            }
        }
        else
        {
            Disconnect();
        }
    }
    #endregion
}
```

---

## Connector

- 공식적인 명칭 X
- |  Server  |  Client   |
  | :------: | :-------: |
  | Listener | Connector |
- 분산 서버 환경에서는 서버끼리도 통신하기 위해 `Connector`가 필수!

### Connector 예제

```cs
public class Connector
{
    // 어떤 Session인 지 모르므로 Contents 단에서 만든 Session을 Invoke()
    Func<Session> _sessionFactory;

    public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory)
    {
        _sessionFactory = sessionFactory;

        // 휴대폰 설정: Socket()
        Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // For. Async
        SocketAsyncEventArgs args = new SocketAsyncEventArgs();

        // 완료 이벤트 등록
        args.Completed += OnConnectCompleted;
        args.RemoteEndPoint = endPoint;

        // 넘겨줄 정보를 담음.
        // object type으로 받으므로 모든 type이 가능
        args.UserToken = socket;

        // 예약
        RegisterConnect(args);
    }

    void RegisterConnect(SocketAsyncEventArgs args)
    {
        // object형이므로 Casting
        Socket socket = args.UserToken as Socket;
        if (socket == null) return;

        bool pending = socket.ConnectAsync(args);
        if (pending == false)
            OnConnectCompleted(null, args);
    }
    void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            // Contents 단에서 요구하는 방식대로 Session을 생성
            Session session = _sessionFactory.Invoke();
            session.Start(args.ConnectSocket);  // 연결된 Socket

            session.OnConnected(args.RemoteEndPoint);
        }
        else
        {
            Console.WriteLine($"OnConnectCompleted Fail: {args.SocketError}");
        }
    }
}
```

---

## TCP vs UDP

- Network 통신은 `Packet`을 주고 받는다.

  > Class를 작성하여 원하는 정보만 주고 받음.

- `TCP` vs `UDP`

  | `TCP`(Transmission Control Protocol) | `UDP`(User Datagram Protocol) |
  | :----------------------------------: | :---------------------------: |
  |     전화 연결 방식, 안전성 높음      |    우편 전송 방식, 위험함     |
  |            연결형 서비스             |        비연결형 서비스        |
  |          전송 순서 보장 [O]          |      전송 순서 보장 [X]       |
  |      속도 느림: UDP에 비교해서       |           속도 빠름           |
  |    신뢰성 높음: 손실 시 다시 보냄    |          신뢰성 낮음          |
  |           흐름 / 혼잡 제어           |                               |
  |                                      |       ex) 주로 FPS 게임       |

---

## Buffer

- TCP 통신할 경우, 끊어진 패킷에 대해 부분적으로 처리할 수 있도록 해야 한다.

### Receive Buffer

- `void Clean()`
- `bool OnRead(int numOfBytes)`
  > Read할 경우를 처리  
  > read cursor를 옮김  
  > 2byte 읽을 때: [r][][][][w] -> [][][r][][w]  
  > Error or Success
- `bool OnWrite(int numOfBytes)`
  > Write할 경우를 처리  
  > Write cursor를 옮김  
  > Data가 4byte 들어올 때: [rw][][][][] -> [r][][][][w]  
  > Error or Success

```cs
public class RecvBuffer
{
    // [rw][][][][][][][]
    ArraySegment<byte> _buffer; // buffer, C#은 Segment로 사용(pointer 불가)
    int _readPos; // read cursor
    int _writePos;  // write cursor

    public RecvBuffer(int bufferSize)   // 생성자
    {
        _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
    }

    // Property: Data크기, 남은 공간 크기
    public int DataSize { get { return _writePos - _readPos; } }
    public int FreeSize { get { return _buffer.Count - _writePos; } }

    // 처리할 Segment 리턴
    public ArraySegment<byte> ReadSegment
    {
        get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
    }
    // 남은 공간 Segment 리턴
    public ArraySegment<byte> WriteSegment
    {
        get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
    }

    public void Clean()
    {
        int dataSize = DataSize;
        if (dataSize == 0)
        {
            // 남은 데이터가 없을 경우
            // [rw] 겹쳐 있을 때: 커서 위치만 초기로
            _readPos = _writePos = 0;
        }
        else
        {
            // [][][r][][w] -> [r][][w][][]
            // 남은 데이터가 있을 때: 데이터 크기를 시작 위치로 복사
            // Copy(SourceArray, Source 위치, DestinationArray, Destination위치, 복사할 크기)
            Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
            _readPos = 0;
            _writePos = dataSize;
        }
    }

    public bool OnRead(int numOfBytes)  // Read 할 경우
    {
        if (numOfBytes > DataSize)
            return false;

        // read cursor 이동
        _readPos += numOfBytes;
        return true;
    }

    public bool OnWrite(int numOfBytes) // Write 할 경우
    {
        if (numOfBytes > FreeSize)
            return false;

        // write cursor 이동
        _writePos += numOfBytes;
        return true;
    }
}
```

- in Session.cs

```cs
void ResgisterRecv()
{
    _recvBuffer.Clean();  // 커서가 너무 뒤로 이동하는 것을 방지
    ArraySegment<byte> segment = _recvBuffer.WriteSegment;  // 남은 세그먼트 공간 리턴

    _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

    // ...
}

void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
{
    // ...
    // 성공적으로 Data를 가져옴
    // Write 커서 이동
    if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
    {
        // 버그 발생
        Disconnect();
        return;
    }

    // 컨텐츠 단으로 데이터를 넘겨주고 얼마나 처리했는지 받음
    int processLen = OnRecv(_recvBuffer.ReadSegment);
    if (processLen < 0 || _recvBuffer.DataSize < processLen)
    {
        // 버그 발생: 컨텐츠 단에서 이상하게 넣을 경우
        Disconnect();
        return;
    }

    // Read 커서 이동
    if (_recvBuffer.OnRead(processLen) == false)
    {
        // 버그 발생
        Disconnect();
        return;
    }

    ResgisterRecv();
}
```

### Send Buffer

- Send()는 Receive()와 다르게 모든 사용자에게 동일한 상황을 주기 위해 많이 실행된다.
  > Send Buffer를 Copy해서 사용하게 되면 Copy 비용이 많이 들기 때문에 최적화가 필요.  
  > 뭉텅이(Chunk)로 만들고 쪼개서 사용하는 방식으로 최적화.
- Send Buffer는 일회용

  > 다른 Session에서 Send를 위해 Queue에 집어넣은 상태일 수도 있으므로 Clean 불가능

- `ArraySegement<byte> Open(int reserveSize)`
  > 예약된 공간을 리턴
- `ArraySegement<byte> Close(int usedSize)`
  > 실질적으로 사용할 공간을 리턴

```cs
// SendBuffer를 예약과 생성을 도와주는 클래스
public class SendBufferHelper
{
    // Thread의 고유한 전역 공간
    // Thread끼리 경합할 필요 X
    public static ThreadLocal<SendBuffer> CurrentBuffer = new ThreadLocal<SendBuffer>(() => null);
    // 뭉텅이(Chunk)로 할당하고 쪼개서 사용
    public static int ChunkSize { get; set; } = 4096 * 100;

    public static ArraySegment<byte> Open(int reserveSize)
    {
        if (CurrentBuffer.Value == null)
            CurrentBuffer.Value = new SendBuffer(ChunkSize);

        // 해당 구문은 이전 SendBuffer 객체를 지움.
        // Pooling 방식으로 재사용으로 조금 더 최적화 가능.
        if (CurrentBuffer.Value.FreeSize < reserveSize)
            CurrentBuffer.Value = new SendBuffer(ChunkSize);

        return CurrentBuffer.Value.Open(reserveSize);
    }

    public static ArraySegment<byte> Close(int usedSize)
    {
        return CurrentBuffer.Value.Close(usedSize);
    }
}

// 실질적인 Send-Buffer
public class SendBuffer
{
    // [u][][][][][][][]
    byte[] _buffer;     // buffer
    int _usedSize = 0;  // used cursor

    public SendBuffer(int chunkSize)
    {
        _buffer = new byte[chunkSize];
    }

    // Property: 남은 공간 크기
    public int FreeSize { get { return _buffer.Length - _usedSize; } }

    public ArraySegment<byte> Open(int reserveSize)
    {
        if (reserveSize > FreeSize)
            return null;

        return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
    }

    public ArraySegment<byte> Close(int usedSize)
    {
        ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSize);
        _usedSize += usedSize;

        return segment;
    }
}
```

#### Serialize without send-buffer

```cs
class Knight
{
    public int hp;
    public int attack;
}

public override void OnConnected(EndPoint endPoint)
{
    Knight knight = new Knight() { hp = 100, attack = 10 };

    byte[] sendBuff = new byte[1024];
    byte[] buffer1 = BitConverter.GetBytes(knight.hp);  // int -> byte[]
    byte[] buffer2 = BitConverter.GetBytes(knight.attack);  // int -> byte[]

    Array.Copy(buffer1, 0, sendBuff, 0, buffer1.Length);
    Array.Copy(buffer2, 0, sendBuff, buffer1.Length, buffer2.Length);

    Send(sendBuff);
}
```

#### Serialize with send-buffer

```cs
class Knight
{
    public int hp;
    public int attack;
}

public override void OnConnected(EndPoint endPoint)
{
    Knight knight = new Knight() { hp = 100, attack = 10 };

    ArraySegment<byte> openSegment = SendBufferHelper.Open(4096); // 공간 예약
    byte[] buffer1 = BitConverter.GetBytes(knight.hp);
    byte[] buffer2 = BitConverter.GetBytes(knight.attack);

    Array.Copy(buffer1, 0, openSegment.Array, openSegment.Offset, buffer1.Length);
    Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer1.Length, buffer2.Length);

    // 실질적인 버퍼를 부여받음
    ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer1.Length + buffer2.Length);

    Send(sendBuff);
}
```

---

## Packet Session

### Packet

- 실질적으로 Data를 Packet으로 주고 받는다.
- Packet은 최대한 압축해서 보내야된다.

  > Serialized: 직렬화  
  > 패킷을 이어붙여서 보냄.
  > [size(2)][packetid(2)][...][size(2)][packetId(2)][...]

#### Packet Header

1. 패킷 크기
   > 얼마나 보냈는 지, 부분적으로 왔는 지 판별 가능
2. 패킷 Id
   > 어떤 Packet인 지, 유효한 지 판별

```cs
class Packet
{
    // uint(4 byte) vs ushort(2 byte)
    // 기본적으로 ushort로 충분
    public ushort size; // 패킷 크기
    public ushort packetId; // 패킷 ID
}
```

#### LoginOkPacket 예제

```cs
class LoginOkPacket: Packet
{
    // 상속 때문에 size, packetId 멤버 변수를 가지고 있음
    public int ok;
}
```

### PacketSession

```cs
public abstract class PacketSession : Session
{
    public static readonly int HeadSize = 2;

    // sealed: 더 이상 override 불가
    // PacketSession을 상속받아도 OnRecv() 구현 불가
    public sealed override int OnRecv(ArraySegment<byte> buffer)
    {
        // if. packet: [size(2)][packetId(2)][...][size(2)][packetId(2)][...]
        int processLen = 0; // 처리한 length

        // Parsing packet
        while (true)
        {
            // 최소 헤더 파싱 여부
            if (buffer.Count < HeadSize)
                break;

            // Packet이 완전체로 도착했는지 여부
            ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            if (buffer.Count < dataSize)
                break;

            // Packet 조립
            // [size(2)][packetId(2)][...] 넘겨줌.
            // buffer.Slice() API도 가능.
            // ArraySegment는 stack 영역(= struct)이므로
            OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));

            processLen += dataSize;

            // 다음 buffer slice
            buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
        }

        return processLen;  // 처리한 length
    }

    // PacketSession을 상속 받는 Class는 이 인터페이스를 override
    public abstract void OnRecvPacket(ArraySegment<byte> buffer);
}
```

---

## UTF-8 vs UTF-16

- Encoding
  > 문자를 컴퓨터가 이해시키기 위해 만듦.
- Server와 Client는 UTF-8 or UTF-16 Encoding 규격을 맞춰야 통신이 가능.

### 고정(Fix)

#### ASCII Code

- 0~127
- 1byte로 표현
- Only. English
- ex)

  1. `A`: 0x41, 65
  2. `!`: 0x21, 33

#### Unicode

- 0~65535
- 2byte로 표현(= BMP 영역) + 1byte 추가(한문 등)
- the other language
- ex)

  1. `A`: 0x000041, 65
  2. `!`: 0x000021, 33
  3. `ㅎ`: 0x001112, 4370

### 가변(no Fix)

#### UTF-8

- 영어권에 유리

  - 영문: 1byte로 표현
  - 한글: 3byte로 표현

- ex)

  1. C++

#### UTF-16

- 한글, 중국어, 일본어 등등 유리

  - 영문: 2byte로 표현
  - 한글: 2byte로 표현
  - BMP 영역 외: 4byte로 표현

- ex)

  1. C#

---

## Serialization

- Serialization: 직렬화

  > 객체 형태의 패킷을 버퍼 안에 밀어넣는 작업  
  > Instance -> byte[]  
  > byte[]를 buffer에 밀어넣음.  
  > 네트워크 송수신 가능하도록 함.

- Deserialization: 역직렬화

  > 버퍼 안에 패킷을 객체 형태로 읽는 작업  
  > byte[] -> Instance  
  > buffer의 내용을 Instance로 읽음

- Packet
  > 일종의 Class의 Instance  
  > 보통 `.json`, `.xml` 파일로 자동화하여 정의.

### primitive type

- Packet class

  ```cs
  public abstract class Packet
  {
      public ushort size;
      public ushort packetId;

      public abstract ArraySegment<byte> Write();
      public abstract void Read(ArraySegment<byte> seg);
  }

  class PlayerInfoReq : Packet
  {
      public long playerId;

      public override ArraySegment<byte> Write(){ }
      public override void Read(ArraySegment<byte> seg){ }
  }
  ```

#### Serialization: primitive type

##### `GetBytes()`, `Array.Copy()`

- 안정적, but. 성능이 조금 떨어짐.
- GetBytes(value)
- Copy(sourceArray, sourceIndex, destArray, destIndex, length)

```cs
public ArraySegment<byte> Write()
{
    // 버퍼 공간 예약
    ArraySegment<byte> seg = SendBufferHelper.Open(4096);

    // byte[] get
    // header
    byte[] size = BitConverter.GetBytes(packet.size);    // ushort: 2
    byte[] packetId = BitConverter.GetBytes(packet.packetId);    // ushort: 2

    // data
    byte[] playerId = BitConverter.GetBytes(packet.playerId);   // long: 8

    // Serialization
    ushort count = 0;   // Offset 처리

    count += sizeof(ushort);  // packetSize type
    Array.Copy(packetId, 0, seg.Array, seg.Offset + count, 2);
    count += sizeof(ushort);  // packetId type
    Array.Copy(playerId, 0, seg.Array, seg.Offset + count, 8);
    count += sizeof(long);  // playerId type

    // packcet size는 마지막에 알 수 있으므로
    Array.Copy(size, 0, seg.Array, seg.Offset, 2);

    // 실패할 경우, null 리턴
    if (success == false)
        return null;

    // 성공할 경우, 실제 공간 리턴
    return SendBufferHelper.Close(count);
}
```

##### `TryWriteBytes()`, `Span<byte>()`

- GetBytes()의 성능을 향상시킴.
- bool TryWriteBytes(destination, value)
- new Span\<byte\>(array, offset, count)

```cs
public ArraySegment<byte> Write()
{
    // 버퍼 공간 예약
    ArraySegment<byte> seg = SendBufferHelper.Open(4096);

    ushort count = 0;   // Offset 계산
    bool success = true;  // 최종적으로 성공 여부

    count += sizeof(ushort); // packetSize type
    success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset + count, seg.Count - count), packet.packetId);
    count += sizeof(ushort); // packetId type
    success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset + count, seg.Count - count), packet.playerId);
    count += sizeof(long); // playerId type

    // packcet size는 마지막에 알 수 있으므로
    success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset, seg.Count), count);

    // 실패할 경우, null 리턴
    if (success == false)
        return null;

    // 성공할 경우, 실제 공간 리턴
    return SendBufferHelper.Close(count);
}
```

##### `TryWriteBytes()`, `Slice()`

- `Span`을 잘라서 사용: 직관적임
- bool TryWriteBytes(destination, value)
- Slice(start, length)

```cs
public ArraySegment<byte> Write()
{
    // 버퍼 공간 예약
    ArraySegment<byte> seg = SendBufferHelper.Open(4096);
    Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);

    ushort count = 0;
    bool success = true;

    count += sizeof(ushort);  // packetSize type
    success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.packetId);
    count += sizeof(ushort);  // packetId type
    success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.playerId);
    count += sizeof(long);  // playerId type

    // packcet size는 마지막에 알 수 있으므로
    success &= BitConverter.TryWriteBytes(span, count);

    // 실패할 경우, null 리턴
    if (success == false)
        return null;

    // 성공할 경우, 실제 공간 리턴
    return SendBufferHelper.Close(count);
}
```

#### Deserialization: primitive type

- Client에서 패킷을 속여 보낸다고 생각하고 코드를 작성해야 함.
- 충분한 공간이 있는지 `ReadOnlySpan`으로 검사

- BitConverter

  - `ToInt16`: `signed short`(16bits = 2bytes)
  - `ToUInt16`: `unsigned short`(16bits = 2bytes)

  - `ToInt32`: `signed int`(32bits = 4bytes)
  - `ToUInt32`: `unsigned int`(32bits = 4bytes)

  - `ToSingle`: `float`
  - `ToDouble`: `double`

##### `Segment Offset`

- 범위 초과에 대해 위험하다.

```cs
public void Read(ArraySegment<byte> seg)
{
    ushort count = 0;

    ushort size = BitConverter.ToUInt16(seg.Array, seg.Offset);
    count += sizeof(ushort);
    ushort id = BitConverter.ToUInt16(seg.Array, seg.Offset + count);
    count += sizeof(ushort);
    long playerId = BitConverter.ToInt64(seg.Array, seg.Offset + count);
    count += sizeof(long);
}
```

##### `ReadOnlySpan`

- 범위를 잘라서 사용하여 안정적임.
- 범위가 초과할 시에 `Exception` 발생

```cs
public void Read(ArraySegment<byte> seg)
{
    ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);

    ushort count = 0;

    ushort size = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
    count += sizeof(ushort);
    ushort packetId = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
    count += sizeof(ushort);
    long playerId = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
    count += sizeof(long);
}
```

### string type, byte[] type

- string은 크기를 예상할 수 없다.
- `C++`(UTF-8) 기준에서는 `\0`로 판별하여 보냄
- `C#`(UTF-16) 기준에서는 `string length` + `string data`로 판별

  - `byte[2](ushort)` + `byte[n](data)`

- `C#`은 기본적으로 UTF-16(Unicode)를 사용

- Packet class

  ```cs
  public abstract class Packet
  {
      public ushort size;
      public ushort packetId;

      public abstract ArraySegment<byte> Write();
      public abstract void Read(ArraySegment<byte> seg);
  }

  class PlayerInfoReq : Packet
  {
      public string name;

      public override ArraySegment<byte> Write(){ }
      public override void Read(ArraySegment<byte> seg){ }
  }
  ```

#### Serialization: string type, byte[] type

##### `GetByteCount`, `Array.Copy()`

- int GetByteCount(string)
  > `length`를 리턴

```cs
public ArraySegment<byte> Write()
{
    // 버퍼 공간 예약
    ArraySegment<byte> seg = SendBufferHelper.Open(4096);
    Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);

    ushort count = 0;   // offset
    bool success = true;  // 성공 여부

    count += sizeof(ushort);  // packet size
    count += sizeof(ushort);  // packet Id

    // string length [2](ushort)
    // UTF-16(=Unicode)
    ushort nameLen = (ushort)Encoding.Unicode.GetByteCount(this.name);
    success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);
    count += sizeof(ushort);  // ushort type

    // string byte[n](data)
    Array.Copy(Encoding.Unicode.GetBytes(this.name), 0, seg.Array, count, nameLen);
    count += nameLen;   // string byte count

    // packcet size는 마지막에 알 수 있으므로
    success &= BitConverter.TryWriteBytes(span, count);

    // 실패할 경우, null 리턴
    if (success == false)
        return null;

    // 성공할 경우, 실제 공간 리턴
    return SendBufferHelper.Close(count);
}
```

##### `GetBytes()`

- int GetBytes(source, source offset, length, destination, destination offset)
  > `length`를 리턴

```cs
public ArraySegment<byte> Write()
{
    // 버퍼 공간 예약
    ArraySegment<byte> seg = SendBufferHelper.Open(4096);
    Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);

    ushort count = 0;   // offset
    bool success = true;  // 성공 여부

    count += sizeof(ushort);  // packet size
    count += sizeof(ushort);  // packet Id

    // byte[2](ushort), byte[n](data)
    ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, seg.Array, seg.Offset + count + sizeof(ushort));
    success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), nameLen);

    count += sizeof(ushort);    // ushort type
    count += nameLen;   // string byte count

    // packcet size는 마지막에 알 수 있으므로
    success &= BitConverter.TryWriteBytes(span, count);

    // 실패할 경우, null 리턴
    if (success == false)
        return null;

    // 성공할 경우, 실제 공간 리턴
    return SendBufferHelper.Close(count);
}
```

#### Deserialization: string type, byte[] type

##### `GetString()`

- string GetString(span)

```cs
public void Read(ArraySegment<byte> seg)
{
    ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);

    ushort count = 0;

    // string
    ushort nameLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
    count += sizeof(ushort);

    this.name = Encoding.Unicode.GetString(span.Slice(count, nameLen));
    count += nameLen;
}
```

### list, struct type

- list
  - string과 마찬가지로, `list count` + `list date` 로 보낸다.
- struct

  - 내부의 함수로 Serialization, Deserialization 처리

- Packet class

  ```cs
  public abstract class Packet
  {
      public ushort size;
      public ushort packetId;

      public abstract ArraySegment<byte> Write();
      public abstract void Read(ArraySegment<byte> seg);
  }

  class PlayerInfoReq : Packet
  {
      public List<SkillInfo> skills = new List<SkillInfo>();

      public struct SkillInfo
      {
          public int id;
          public short level;
          public float duration;

          public bool Write(Span<byte> span, ref short count){ }
          public void Read(ReadOnlySpan<byte> span, ref ushort count){ }
      }

      // struct 내부적으로 처리
      public bool ArraySegment<byte> Write()
      {
          bool success = true;

          success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.id);
          count += sizeof(int);
          success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.level);
          count += sizeof(short);
          success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.duration);
          count += sizeof(float);

          return true;
      }

      // struct 내부적으로 처리
      public void Read(ArraySegment<byte> seg)
      {
          this.id = BitConverter.ToInt32(span.Slice(count, span.Length - count));
          count += sizeof(int);
          this.level = BitConverter.ToInt16(span.Slice(count, span.Length - count));
          count += sizeof(short);
          this.duration = BitConverter.ToSingle(span.Slice(count, span.Length - count));
          count += sizeof(float);
      }
  }
  ```

#### Serialization: list, struct type

```cs
public ArraySegment<byte> Write()
{
    // 예약
    ArraySegment<byte> seg = SendBufferHelper.Open(4096);
    Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);

    ushort count = 0;   // offset
    bool success = true;  // 성공 여부

    count += sizeof(ushort);  // packet size
    count += sizeof(ushort);  // packet Id

    // Skill list
    // Skill list length
    success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)skills.Count);
    count += sizeof(ushort);

    // Skill list data: count는 내부적으로 처리
    foreach (SkillInfo skill in skills)
        success &= skill.Write(span, ref count);

    // packcet size는 마지막에 알 수 있으므로
    success &= BitConverter.TryWriteBytes(span, count);

    // 실패할 경우, null 리턴
    if (success == false)
        return null;

    // 성공할 경우, 실제 공간 리턴
    return SendBufferHelper.Close(count);
}
```

#### Deserialization: list, struct type

```cs
public void Read(ArraySegment<byte> seg)
{
    // 예약
    ArraySegment<byte> seg = SendBufferHelper.Open(4096);
    Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);

    ushort count = 0;   // offset
    bool success = true;  // 성공 여부

    count += sizeof(ushort);  // packet size
    count += sizeof(ushort);  // packet Id

    // 혹시 데이터 들어있는 경우 방지
    skills.Clear();

    // Skill list
    ushort skillLen = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
    count += sizeof(ushort);

    // Skill data
    for (int i = 0; i < skillLen; i++)
    {
        SkillInfo skill = new SkillInfo();
        skill.Read(span, ref count);
        skills.Add(skill);
    }
}
```

### Protocl buffers vs Flat buffers

- Protocal buffers
  > 중간에 Inastance를 만들어 채우고 변환하는 작업  
  > 직관적임  
  > 구조화된 데이터를 직렬화
- Flat buffers
  > 데이터를 byte[]에 직접적으로 넣는 작업  
  > 플랫폼에 상관없음  
  > 게임 개발에 많이 사용

---

## Packet Generator

- Packet 생성 자동화

  > Packet Class는 Client, Server 모두 가지고 있어야 한다.  
  > 따라서, 매번 Code를 생성하고 정의할 수 없으므로 자동화로 편의성을 높인다.

- Packet 정의 방법

  1. `.json`
  2. `.xml`
  3. `IDL`: Interface Definition Language

### About 자동화 코드

- Packet class
  > What Packet
- PacketManager
  > Server or Client로부터 받은 Packet Deserializing + Handling
  - ClientPacketManager
    > Server로부터 온 Packet Handling 등록  
    > ex. "S\_"
  - ServerPacketManager
    > Client로부터 온 Packet Handling 등록  
    > ex. "C\_"

#### `Packet.cs`

- Client와 Server가 공통으로 송수신할 Packet 정의

```cs
public enum PacketId
{
    // Random 정책도 가능
    Packet1 = 1,
    Packet2 = 2,
    Packet3 = 3,
}

// Packet Interface
interface IPacket
{
    // Protocal Id
    ushort Protocol { get; }
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}

class Packet1 : IPacket
{
    // 멤버 변수
    // Read(): 멤버 변수 Deserialization
    // Write(): 멤버 변수 Serialization
}
class Packet2 : IPacket
{
    // 멤버 변수
    // Read(): 멤버 변수 Deserialization
    // Write(): 멤버 변수 Serialization
}
class Packet3 : IPacket
{
    // 멤버 변수
    // Read(): 멤버 변수 Deserialization
    // Write(): 멤버 변수 Serialization
}
```

#### `PacketManager.cs`

- Server or CLient로부터 받은 Packet을 Deserializing + Handling
- Server or Client Packet 등록 차이, Class명은 동일

  - `ClientPacketManager.cs`
  - `ServerPacketManager.cs`

- Singleton Pattern

  > 오직 하나의 인스턴스만 존재

- Switch-Case -> Dictionary

  > O(N) -> O(logN): 성능 향상

- 멤버 변수

  - Dictionary\<ushort, Action\<PacketSession, ArraySegment\<byte>>> \_onRecv
    > MakePacket() 콜백으로 실행
  - Dictionary\<ushort, Action\<PacketSession, IPacket>> \_handler
    > Packet Handler 콜백 함수 실행

- 멤버 함수
  - Register()
    > MakePacket() 콜백 등록  
    > Packet Handler 함수 콜백 등록
  - OnRecvPacket()
    > Protocol Id를 읽음  
    > Protocl Id에 해당하는 Packet 조립(Deserialization) 콜백 실행
  - MakePacket()
    > Deserialization Packet  
    > 해당 Packet의 Handler 호출

```cs
class PacketManager
{
    #region Singleton
    // 오직 하나의 인스턴스
    static PacketManager _instance;
    public static PacketManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PacketManager();
            return _instance;
        }
    }
    #endregion

    // <Protocol Id, Receive Handler>
    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    // <Protocol Id, Packet Handler>
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {
        // MakePacket() 등록
        _onRecv.Add((ushort)PacketId.S_Test, MakePacket<S_Test>);
        // Packet Handler 등록
        _handler.Add((ushort)PacketId.S_Test, PacketHandler.S_TestHandler);
    }

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {
        ushort count = 0;

        // Deserializing Packet Id, Packet Size
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        // Packet 조립 콜백 실행
        Action<PacketSession, ArraySegment<byte>> action = null;
        if (_onRecv.TryGetValue(id, out action))
            action.Invoke(session, buffer);
    }

    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {
        // Packet 생성
        T pkt = new T();

        // Deserializing Packet data
        pkt.Read(buffer);

        // Packet Handler 콜백 실행
        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(pkt.Protocol, out action))
            action.Invoke(session, pkt);
    }
}
```

### 패킷 생성 자동화

1. PDL.xml 정의
   > What Packet
2. PacketGenerator/program 실행
   > Packet class, Packet Manager 코드 자동 생성
3. batch 파일(`.bat`) 실행
   > 각 Server, Client Project에 Copy & Paste

#### `GenPackets.bat`

- Only. Windows OS
- 명령어
  - START [실행 Path] [Argument: PDL.xml path]
  - XCOPY [Copy 파일] [Paste 파일]
    > `/Y`: 덮어 쓸지 확인 묻지 않기.

```bat
START ../PacketGenerator/bin/Debug/PacketGenerator.exe ../PacketGenerator/PDL.xml
XCOPY /Y Packet.cs "../../client/dummy/packet"
XCOPY /Y Packet.cs "../../server/test/packet"
XCOPY /Y ClientPacketManager.cs "../../client/dummy/packet"
XCOPY /Y ServerPacketManager.cs "../../server/test/packet"
```

#### `PDL.xml`

- Packet Definition List
  - \<packet> ~ \</packet>: 패킷 정의
  - \<long>, \<string>, \<list>: Type 정의
  - [C_]... Client -> Server Packet
  - [S_]... Server -> Client Packet

```cs
<?xml version="1.0" encoding="utf-8" ?>
<PDL>
  <packet name="C_PlayerInfoReq">
    <byte name="testByte"/>
    <long name="playerId"/>
    <string name="name"/>
    <list name="skill">
      <int name="id"/>
      <short name="level"/>
      <float name="duration"/>
      <list name="attribute">
        <int name="att"/>
      </list>
    </list>
  </packet>
  <packet name="S_Test">
    <int name="testInt"/>
  </packet>
</PDL>
```

#### PacketFormat.cs

- Packet을 자동화를 위한 String Format
  - managerFormat
    > PacketManager.cs 전체적인 윤곽
  - managerRegistFormat
    > Handler 콜백 Func 등록 in PacketManager.cs
  - fileFormat
    > Packet.cs 전체적인 윤곽
  - packetEnumFormat
    > Packet Protocal Id 정의
  - packetFormat
    > Packet class 정의
  - memberFormat
    > Packet 안의 Primitive type의 멤버 변수 정의
  - memberListFormat
    > Packet 안의 List type의 멤버 변수 정의
  - readFormat
    > Primitive type(byte 제외) 멤버 변수 Deserialization 구문 정의
  - readByteFormat
    > Byte type 멤버 변수 Deserialization 구문 정의
  - readStringFormat
    > String type 멤버 변수 Deserialization 구문 정의
  - readListFormat
    > List type 멤버 변수 Deserialization 구문 정의
  - writeFormat
    > Primitive type(byte 제외) 멤버 변수 Serialization 구문 정의
  - writeByteFormat
    > Byte type 멤버 변수 Serialization 구문 정의
  - writeStringFormat
    > String type 멤버 변수 Deserialization 구문 정의
  - writeListFormat
    > List type 멤버 변수 Deserialization 구문 정의

```cs
using System;

namespace tools
{
    class PacketFormat
    {
        // {0} 패킷 등록
        public static string managerFormat =
@"using System;
using System.Collections.Generic;
using core;

class PacketManager
{{
    #region Singleton
    static PacketManager _instance;
    public static PacketManager Instance
    {{
        get
        {{
            if (_instance == null)
                _instance = new PacketManager();
            return _instance;
        }}
    }}
    #endregion

    Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>>>();
    Dictionary<ushort, Action<PacketSession, IPacket>> _handler = new Dictionary<ushort, Action<PacketSession, IPacket>>();

    public void Register()
    {{
{0}
    }}

    public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
    {{
        ushort count = 0;

        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        Action<PacketSession, ArraySegment<byte>> action = null;
        if (_onRecv.TryGetValue(id, out action))
            action.Invoke(session, buffer);

    }}

    void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer) where T : IPacket, new()
    {{
        T pkt = new T();
        pkt.Read(buffer);

        Action<PacketSession, IPacket> action = null;
        if (_handler.TryGetValue(pkt.Protocol, out action))
            action.Invoke(session, pkt);
    }}
}}
";
        // {0} 패킷 이름
        public static string managerRegistFormat =
@"        _onRecv.Add((ushort)PacketId.{0}, MakePacket<{0}>);
        _handler.Add((ushort)PacketId.{0}, PacketHandler.{0}Handler);";



        // {0} 패킷 이름/번호 목록
        // {1} 패킷 목록
        public static string fileFormat =
@"using System;
using System.Collections.Generic;
using System.Text;
using core;

public enum PacketId
{{
    {0}
}}

interface IPacket
{{
    ushort Protocol {{ get; }}
    void Read(ArraySegment<byte> segment);
    ArraySegment<byte> Write();
}}

{1}
";

        // {0} 패킷 이름
        // {1} 패킷 번호
        public static string packetEnumFormat =
@"{0} = {1},";

        // {0} 패킷 이름
        // {1} 멤버 변수들
        // {2} 멤버 변수 Read
        // {3} 멤버 변수 Write
        public static string packetFormat =
@"
class {0} : IPacket
{{
    {1}
    public ushort Protocol {{ get {{ return (ushort)PacketId.{0}; }} }}

    public void Read(ArraySegment<byte> seg)
    {{
        ushort count = 0;

        ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);
        count += sizeof(ushort);
        count += sizeof(ushort);

        {2}
    }}

    public ArraySegment<byte> Write()
    {{
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);
        ushort count = 0;
        bool success = true;

        Span<byte> span = new Span<byte>(seg.Array, seg.Offset, seg.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)PacketId.{0});
        count += sizeof(ushort);

        {3}

        success &= BitConverter.TryWriteBytes(span, count);     // 원본

        if (success == false)
            return null;

        return SendBufferHelper.Close(count);
    }}
}}
";

        // {0} 변수 형식
        // {1} 변수 이름
        public static string memberFormat =
@"public {0} {1};";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        // {2} 멤버 변수들
        // {3} 멤버 변수 Read
        // {4} 멤버 변수 Write
        public static string memberListFormat =
@"
public class {0}
{{
    {2}

    public void Read(ReadOnlySpan<byte> span, ref ushort count)
    {{
        {3}
    }}

    public bool Write(Span<byte> span, ref ushort count)
    {{
        bool success = true;

        {4}

        return true;
    }}
}}
public List<{0}> {1}s = new List<{0}>();
";

        // {0} 변수 이름
        // {1} To- 변수 형식
        // {2} 변수 형식
        public static string readFormat =
@"this.{0} = BitConverter.{1}(span.Slice(count, span.Length - count));
count += sizeof({2});";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string readByteFormat =
@"this.{0} = ({1})seg.Array[seg.Offset + count];
count += sizeof({1});";

        // {0} 변수 이름
        public static string readStringFormat =
@"ushort {0}Len = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
count += sizeof(ushort);
Encoding.Unicode.GetString(span.Slice(count, {0}Len));
count += {0}Len;";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        public static string readListFormat =
@"this.{1}s.Clear();
ushort {1}Len = BitConverter.ToUInt16(span.Slice(count, span.Length - count));
count += sizeof(ushort);
for (int i = 0; i < {1}Len; i++)
{{
    {0} {1} = new {0}();
    {1}.Read(span, ref count);
    {1}s.Add({1});
}}";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string writeFormat =
@"success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), this.{0});
count += sizeof({1});";

        // {0} 변수 이름
        // {1} 변수 형식
        public static string writeByteFormat =
@"seg.Array[seg.Offset + count] = (byte)this.{0};
count += sizeof({1});";

        // {0} 변수 이름
        public static string writeStringFormat =
@"ushort {0}Len = (ushort)Encoding.Unicode.GetBytes(this.{0}, 0, this.{0}.Length, seg.Array, seg.Offset + count + sizeof(ushort));
success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), {0}Len);
count += sizeof(ushort);
count += {0}Len;";

        // {0} 리스트 이름 [대문자]
        // {1} 리스트 이름 [소문자]
        public static string writeListFormat =
@"success &= BitConverter.TryWriteBytes(span.Slice(count, span.Length - count), (ushort)this.{1}s.Count);
count += sizeof(ushort);
foreach ({0} {1} in this.{1}s)
    success &= {1}.Write(span, ref count);";
    }
}
```

#### PacketGenerator/Program.cs

- Program
  - Main()
    > XML parsing
  - ParsePacket()
    > Packet class를 parsing
  - ParseMembers()
    > Packet class의 멤버 변수 parsing
  - ParseList()
    > Packet class의 List type parsing
  - ToMemberType()
    > Encoding 정의(for. Deserializing)
  - FirstCharToUpper()
    > List type Naming
  - FirstCharToLower()
    > List variable Naming

```cs
class Program
{
    static string packetClass;   // Packet class
    static ushort packetId;     // Packet Protocol Id
    static string packetEnums;  // Packet enum 지정

    static string clientRegister;   // for. ClientPacketManager
    static string serverRegister;   // for. ServerPacketManager

    static void Main(string[] args)
    {
        // PDL Path
        string pdlPath = "PDL.xml";

        // 환경 설정
        XmlReaderSettings settings = new XmlReaderSettings()
        {
            IgnoreComments = true,  // 주석 무시
            IgnoreWhitespace = true,  // 스페이스바 무시
        };

        // Packet Generator Program을 실행할 시 argument로 PDL.xml Path 지정
        if (args.Length >= 1)
            pdlPath = args[0];

        // 자동화
        using (XmlReader reader = XmlReader.Create(pdlPath, settings))
        {
            // 헤더를 건너 뛰고 핵심 내용부터
            reader.MoveToContent();

            // PDL parsing: String으로 읽음
            while (reader.Read())
            {
                // Depth가 0: <PDL>, </PDL>
                // XmlNodeType.Element: <packet>
                // XmlNodeType.EndElement: </packet>
                if (reader.Depth == 1 && reader.NodeType == XmlNodeType.Element)
                    ParsePacket(reader);
            }
            // Packet.cs
            string fileText = string.Format(PacketFormat.fileFormat, packetEnums, packetClass);
            File.WriteAllText("Packets.cs", fileText);

            // ClientManager.cs
            string clientManagerText = string.Format(PacketFormat.managerFormat, clientRegister);
            File.WriteAllText("ClientPacketManager.cs", clientManagerText);

            // ServerManager.cs
            string serverManagerText = string.Format(PacketFormat.managerFormat, serverRegister);
            File.WriteAllText("ServerPacketManager.cs", serverManagerText);
        }
    }

    static void ParsePacket(XmlReader reader)
    {
        // </> 일 경우 return
        if (reader.NodeType == XmlNodeType.EndElement)
            return;

        // <packet>이 아닐 경우 return
        if (reader.Name.ToLower() != "packet")
        {
            Console.WriteLine("Invalid packet node");
            return;
        }

        // <packet name="">일 경우 return
        string packetName = reader["name"];
        if (string.IsNullOrEmpty(packetName))
        {
            Console.WriteLine("Packet without name");
            return;
        }

        // Packet.cs format append
        Tuple<string, string, string> tuple = ParseMembers(reader);
        packetClass += string.Format(PacketFormat.packetFormat, packetName, tuple.Item1, tuple.Item2, tuple.Item3);

        // Packet.cs의 enum 증가
        packetEnums += string.Format(PacketFormat.packetEnumFormat, packetName, ++packetId) + Environment.NewLine + "\t";

        // ClientPacketManager: Client's Packet Handler를 다룸, "S_": Server's Packet
        // ServerPacketManager: Server's Packet Handler를 다룸.
        if (packetName.StartsWith("S_") || packetName.StartsWith("s_"))
            clientRegister += string.Format(PacketFormat.managerRegistFormat, packetName) + Environment.NewLine;
        else
            serverRegister += string.Format(PacketFormat.managerRegistFormat, packetName) + Environment.NewLine;
    }

    static Tuple<string, string, string> ParseMembers(XmlReader reader)
    {
        string memberCode = ""; // 멤버 변수
        string readCode = "";   // 멤버 변수 Deserializing
        string writeCode = "";  // 멤버 변수Serializing

        int depth = reader.Depth + 1;
        while (reader.Read())
        {
            // packet 안쪽 내용이 아닐 경우 return
            if (reader.Depth != depth)
                break;

            // <type name="">일 경우 return
            string memberName = reader["name"];
            if (string.IsNullOrEmpty(memberName))
            {
                Console.WriteLine("Member without name");
                return null;
            }

            // 개행 추가
            if (string.IsNullOrEmpty(memberCode) == false)
                memberCode += Environment.NewLine;
            if (string.IsNullOrEmpty(readCode) == false)
                readCode += Environment.NewLine;
            if (string.IsNullOrEmpty(writeCode) == false)
                writeCode += Environment.NewLine;

            // type별 parsing
            string memberType = reader.Name.ToLower();
            switch (memberType)
            {
                case "byte":
                case "sbyte":
                    // Only. byte, sbyte type
                    memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                    readCode += string.Format(PacketFormat.readByteFormat, memberName, memberType);
                    writeCode += string.Format(PacketFormat.writeByteFormat, memberName, memberType);
                    break;
                case "bool":
                case "short":
                case "ushort":
                case "int":
                case "long":
                case "float":
                case "double":
                    // Only. bool, short, ushort, int, long, float, double type
                    memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                    readCode += string.Format(PacketFormat.readFormat, memberName, ToMemberType(memberType), memberType);
                    writeCode += string.Format(PacketFormat.writeFormat, memberName, memberType);
                    break;
                case "string":
                    // Only. string type
                    memberCode += string.Format(PacketFormat.memberFormat, memberType, memberName);
                    readCode += string.Format(PacketFormat.readStringFormat, memberName);
                    writeCode += string.Format(PacketFormat.writeStringFormat, memberName);
                    break;
                case "list":
                    // Only. list type
                    Tuple<string, string, string> tuple = ParseList(reader);
                    memberCode += tuple.Item1;
                    readCode += tuple.Item2;
                    writeCode += tuple.Item3;
                    break;
                default:
                    break;
            }
        }
        // 탭, 개행 정리
        memberCode = memberCode.Replace("\n", "\n\t");
        readCode = readCode.Replace("\n", "\n\t\t");
        writeCode = writeCode.Replace("\n", "\n\t\t");

        return new Tuple<string, string, string>(memberCode, readCode, writeCode);
    }

    static Tuple<string, string, string> ParseList(XmlReader reader)
    {
        // <list name="">일 경우 return
        string listName = reader["name"];
        if (string.IsNullOrEmpty(listName))
        {
            Console.WriteLine("List without name");
            return null;
        }

        // list안의 멤버 변수 parsing
        Tuple<string, string, string> tuple = ParseMembers(reader);

        // 멤버 변수
        string memberCode = string.Format(PacketFormat.memberListFormat,
            FirstCharToUpper(listName),
            FirstCharToLower(listName),
            tuple.Item1,
            tuple.Item2,
            tuple.Item3);

        // list 멤버 변수 Deserializing 구문
        string readCode = string.Format(PacketFormat.readListFormat,
            FirstCharToUpper(listName),
            FirstCharToLower(listName));

        // list 멤버 변수 Serializing 구문
        string writeCode = string.Format(PacketFormat.writeListFormat,
            FirstCharToUpper(listName),
            FirstCharToLower(listName));

        return new Tuple<string, string, string>(memberCode, readCode, writeCode);
    }

    static string ToMemberType(string memberType)
    {
        // 멤버 변수별 Encoding
        switch (memberType)
        {
            case "bool":
                return "ToBoolean";
            case "short":
                return "ToInt16";
            case "ushort":
                return "ToUInt16";
            case "int":
                return "ToInt32";
            case "long":
                return "ToInt64";
            case "float":
                return "ToSingle";
            case "double":
                return "ToDouble";
            default:
                return "";
        }
    }

    static string FirstCharToUpper(string input)
    {
        // for. List type naming
        if (string.IsNullOrEmpty(input))
            return "";
        return input[0].ToString().ToUpper() + input.Substring(1);
    }
    static string FirstCharToLower(string input)
    {
        // for. List naming
        if (string.IsNullOrEmpty(input))
            return "";
        return input[0].ToString().ToLower() + input.Substring(1);
    }
}
```

---

## Chat Test

- BroadCast 이용

  > 한 명이 메시지를 보내면 모두에게 보내줘야 한다.  
  > Multi-Thread 환경은 Lock만 잡으면 안정적이만 성능면에서 매우 부하가 심하다.  
  > 실질적으로 Queue에 넣고 다른 일을 하게끔 로직을 만든다.(Job or Task)

- 순서
  1. SessionManager.cs 작성
  2. GameRoom.cs 작성
  3. PDL.xml 변경
  4. PacketHandler 작성

### Chat Test: SessionManager.cs

- Multi-Thread 환경에서 Session과 관련된 코드는 Lock 필요!

- Server's SessionManager

  > ClientSession을 관리

  - `Generate()`
    > ClientSession을 생성 후 `Dictionary<>`로 관리
  - `Find()`
    > `Dictionary<>`에서 ClientSession을 찾음
  - `Remove()`
    > `Dictionary<>`에서 ClientSession을 지움

  ```cs
  class SessionManager
  {
      // Singleton
      static SessionManager _session = new SessionManager();
      public static SessionManager Instance { get { return _session; } }

      int _sessionId = 0;
      Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();

      object _lock = new object();  // for. multi-thread

      public ClientSession Generate()
      {
          lock (_lock)
          {
              int sessionId = ++_sessionId;

              ClientSession session = new ClientSession();
              session.SessionId = sessionId;
              _sessions.Add(sessionId, session);

              Console.WriteLine($"Connected: {sessionId}");

              return session;
          }
      }

      public ClientSession Find(int id)
      {
          lock (_lock)
          {
              ClientSession session = null;
              _sessions.TryGetValue(id, out session);
              return session;
          }
      }

      public void Remove(ClientSession session)
      {
          lock (_lock)
          {
              _sessions.Remove(session.SessionId);
          }
      }
  }
  ```

- Client's SessionManager

  > ServerSession을 관리

  - `Generate()`
    > ServerSession을 생성 후 `List<>`로 관리
  - `SendForEach()`
    > 모든 ServerSession에 Send()

  ```cs
  class SessionManager
  {
      static SessionManager _session = new SessionManager();
      public static SessionManager Instance { get { return _session; } }

      List<ServerSession> _sessions = new List<ServerSession>();
      object _lock = new object();

      public ServerSession Generate()
      {
          lock (_lock)
          {
              ServerSession session = new ServerSession();
              _sessions.Add(session);
              return session;
          }
      }

      public void SendForEach()
      {
          lock (_lock)
          {
              foreach (ServerSession session in _sessions)
              {
                  C_Chat chatPacket = new C_Chat();
                  chatPacket.chat = $"Hello Server !";
                  ArraySegment<byte> segment = chatPacket.Write();

                  session.Send(segment);
              }
          }
      }
  }
  ```

### Chat Test: GameRoom.cs

- 채팅방
- `List<>`로 들어와있는 ClientSession 관리
-
- GameRoom.cs

  - BroadCast()
    > Client의 Packet을 분석  
    > Server의 Packet을 생성 후 모든 Session에게 전송  
    > 병목현상이 일어나는 부분
  - Enter()
    > `List<>`에 들어온 ClientSession 추가
  - Leave()
    > `List<>`에서 ClientSession 제거

  ```cs
  class GameRoom
  {
      List<ClientSession> _sessions = new List<ClientSession>();
      object _lock = new object();

      public void BroadCast(ClientSession session, string chat)
      {
          S_Chat packet = new S_Chat();
          packet.playerId = session.SessionId;
          packet.chat = $"{chat} I am {packet.playerId}";
          ArraySegment<byte> segment = packet.Write();

          lock (_lock)
          {
              foreach (ClientSession s in _sessions)
                  s.Send(segment);
          }
      }

      public void Enter(ClientSession session)
      {
          lock (_lock)
          {
              _sessions.Add(session);
              session.Room = this;
          }
      }

      public void Leave(ClientSession session)
      {
          lock (_lock)
          {
              _sessions.Remove(session);
          }
      }
  }
  ```

### Chat Test: PDL.xml

- C_Chat

  > Client's packet  
  > Client가 보내는 메시지(string chat)

- S_Chat

  > Server's Packet  
  > Server가 보내는 메시지(string chat)

```cs
<?xml version="1.0" encoding="utf-8" ?>
<PDL>
  <packet name="C_Chat">
    <string name="chat"/>
  </packet>
  <packet name="S_Chat">
    <int name="playerId"/>
    <string name="chat"/>
  </packet>
</PDL>
```

### Chat Test: PacketHandler.cs

- Server's Handler

  - C_ChatHandler()
    > Client's Packet(C_Chat) Handling func.  
    > BroadCast: 모든 ClientSession에게 Send()

  ```cs
  class PacketHandler
  {
      public static void C_ChatHandler(PacketSession session, IPacket packet)
      {
          C_Chat chatPacket = packet as C_Chat;
          ClientSession clientSession = session as ClientSession;
          if (clientSession.Room == null)
              return;

          clientSession.Room.BroadCast(clientSession, chatPacket.chat);
      }
  }
  ```

- Client's PacketHandler

  - S_ChatHandler()
    > Server's Packet(S_Chat) Handling func.  
    > Only. 출력

  ```cs
  class PacketHandler
  {
      public static void S_ChatHandler(PacketSession session, IPacket packet)
      {
          S_Chat chatPacket = packet as S_Chat;
          ServerSession serverSession = session as ServerSession;

          // if (chatPacket.playerId == 1)
          Console.WriteLine($"message: {chatPacket.chat}");
      }
  }
  ```

---
