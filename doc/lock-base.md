---
title: "Lock 기초"
category: Game-Server
tags: [lock, interlocked, monitor, lock]
date: "2021-02-09"
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
