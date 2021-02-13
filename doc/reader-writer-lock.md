---
title: "Reader Writer Lock"
category: Game-Server
tags: [reader-writer-lock, rwlock]
date: "2021-02-11"
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
