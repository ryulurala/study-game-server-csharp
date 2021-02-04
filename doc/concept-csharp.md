---
title: "C# 개념 정리"
category: Game-Server
tags: [c#, blocking, non-blocking, call-back, event, action, func]
date: "2021-02-04"
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
