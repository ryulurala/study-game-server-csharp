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
                Interlocked.Increment(ref number);
        }

        static void Thread_2()
        {
            for (int i = 0; i < 100000; i++)
                Interlocked.Decrement(ref number);
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
