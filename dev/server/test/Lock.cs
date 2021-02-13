using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace test
{
    // 재귀적 락을 허용할 지 (Yes) => WriteLock을 잡고 WriteLock을 잡거나 WriteLock을 잡고 ReadLock을 잡음
    // 스핀락 정책(5000번 -> 양보)
    class Lock
    {
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
                _writeCount++;  // 이미 상호-배타적임
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
    }
}