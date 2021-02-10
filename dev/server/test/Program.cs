using System;
using System.Threading;
using System.Threading.Tasks;

namespace test
{
    class SpinLock
    {
        volatile int _locked = 0;

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

        }
        public void Release()
        {
            _locked = 0;    // 잠금 해제
        }
    }
    class Program
    {
        static int _num = 0;
        static SpinLock _lock = new SpinLock();

        static void Thread_1()
        {
            for (int i = 0; i < 1000000; i++)
            {
                _lock.Acquire();
                _num++;
                _lock.Release();
            }
        }
        static void Thread_2()
        {
            for (int i = 0; i < 1000000; i++)
            {
                _lock.Acquire();
                _num--;
                _lock.Release();
            }
        }
        static void Main(string[] args)
        {
            Task task1 = new Task(Thread_1);
            Task task2 = new Task(Thread_2);
            task1.Start();  // 동시에 들어올 때 문제 발생
            task2.Start();

            Task.WaitAll(task1, task2);

            Console.WriteLine($"{_num}");
        }
    }
}
