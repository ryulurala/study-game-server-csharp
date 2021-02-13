using System;
using System.Threading;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        // 자신만의 공간
        static ThreadLocal<string> ThreadName = new ThreadLocal<string>(
            () => $"My Name is {Thread.CurrentThread.ManagedThreadId}"
            );  // Value를 지정

        static void WhoAmI()
        {
            if (ThreadName.IsValueCreated)  // 이미 Name이 있으면
                Console.WriteLine($"{ThreadName.Value}(repeat)");
            else
                Console.WriteLine($"{ThreadName.Value}");
        }

        static void Main(string[] args)
        {
            // 개수 설정
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(3, 3);

            // 병렬 처리: ThreadPool에서 꺼내 쓴다.
            Parallel.Invoke(
                WhoAmI, WhoAmI, WhoAmI,
                WhoAmI, WhoAmI, WhoAmI
                );
        }
    }
}
