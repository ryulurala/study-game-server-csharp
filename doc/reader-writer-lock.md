---
title: "Reader Writer Lock"
category: Game-Server
tags: [reader-writer-lock, rwlock]
date: "2021-02-11"
---

## ReaderWriterLock

- 공유 자원을 조회할 경우와 수정할 경우의 lock 비율이 많이 차이날 경우 사용.
- In Game
  - 게임 아이템은 최종적으로는 DB에 저장되지만 DB부하와 다른 이유로 DB에서 가져와 서버 메모리에서 처리해야 할 경우가 존재.
  - Example: 특정 던전을 유도하기 위해 특정 시간에 운영자가 운영툴로 게임 보상을 추가
    > 보상이 5개에서 6개로 오를 경우, Lock을 걸지 않고는 Data에 수정하기는 어렵다.  
    > 이럴 경우, User들은 Read할 경우는 User의 수만큼이지만 Write는 단 한 번이다.  
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

        // Reward 읽기
        Reward reward = new Reward();

        rwLock.ExitReadLock();

        return reward;
    }

    void addReward()    // 0.0001 %
    {
        rwLock.EnterWriteLock();

        // Reward 추가

        rwLock.ExitWriteLock();
    }
}
```

---
