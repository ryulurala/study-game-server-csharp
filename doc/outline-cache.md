---
title: "캐시 이론"
category: Game-Server
tags: [Cache, CPU, locality]
date: "2021-02-08"
---

## 캐시 이론

- CPU 코어 <-> RAM(메인 메모리)

  > CPU 코어: ALU(연산 장치) + Cache(캐시 장치)  
  > Single-Thread 환경에서는 문제가 없다.  
  > Multi-Thread 환경에서는 각 Thread마다의 정보가 다르다 .

### Locality(지역성)

1. `Temporal Locality` : 시간 - 최근 접근한 것이 또 접근할 확률이 높다.
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
