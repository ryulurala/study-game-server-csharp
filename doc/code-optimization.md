---
title: "코드 최적화 by 컴파일러(S/W)"
category: Game-Server
tags: [c#, code-optimization, debug, release]
date: "2021-02-07"
---

## 컴파일러(S/W) 최적화

### Debug vs Release

|           `Debug`           |          `Release`          |
| :-------------------------: | :-------------------------: |
| 디버깅 정보 포함으로 느리다 | Debug 모드보다 3~4배 빠르다 |
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
  > `volatile`은 `휘발성 데이터`로 코드 작성한 그대로 쓰게 한다.  
  > `C#`과 `C++`에서의 `volatile` 키워드는 다르다.  
  > `C++`에서의 `volatile` 키워드는 최신 값을 가져오는 역할

---
