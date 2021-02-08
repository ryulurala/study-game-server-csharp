---
title: "Interlocked 이용"
category: Game-Server
tags: [interlocked, increment, decrement, exchange, compare-exchange]
date: "2021-02-08"
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
          static int number = 0;
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
