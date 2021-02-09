using System;
using System.Threading;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        class SessionManager
        {
            static object _lock = new object();
            public static void TestSession()
            {
                lock (_lock) { }
            }
            public static void Test()
            {
                lock (_lock) { UserManager.TestUser(); }
            }
        }

        class UserManager
        {
            static object _lock = new object();

            public static void TestUser()
            {
                lock (_lock) { }
            }
            public static void Test()
            {
                lock (_lock) { SessionManager.TestSession(); }
            }
        }


        static void Thread_1()
        {
            for (int i = 0; i < 10000; i++)
            {
                SessionManager.Test();
            }
        }
        static void Thread_2()
        {
            for (int i = 0; i < 10000; i++)
            {
                UserManager.Test();
            }
        }

        static void Main(string[] args)
        {
            Task task1 = new Task(Thread_1);
            Task task2 = new Task(Thread_2);
            task1.Start();

            Thread.Sleep(100);

            task2.Start();

            Task.WaitAll(task1, task2);

            Console.WriteLine("종료!");
        }
    }
}
