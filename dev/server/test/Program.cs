using System;
using System.Threading;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static int _num = 0;
        static Mutex _lock = new Mutex();

        static void Thread_1()
        {
            for (int i = 0; i < 10000; i++)
            {
                _lock.WaitOne();
                _num++;
                _lock.ReleaseMutex();
            }
        }
        static void Thread_2()
        {
            for (int i = 0; i < 10000; i++)
            {
                _lock.WaitOne();
                _num--;
                _lock.ReleaseMutex();
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
