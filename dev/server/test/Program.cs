using System;
using System.Threading;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        // static bool _stop = false;

        volatile static bool _stop = false;
        // 코드 상에서 컴파일러 최적화 하지 마라
        // C++에서는 최신 값을 가져오는 개념(캐시 X)

        static void ThreadMain()
        {
            Console.WriteLine("쓰레드 시작");

            if (!_stop)
            {
                while (true)
                {
                }
            }

            while (!_stop)
            {
                // stop 신호 기다리기
            }

            Console.WriteLine("쓰레드 종료");
        }
        static void Main(string[] args)
        {
            Task task = new Task(ThreadMain);
            task.Start();

            Thread.Sleep(1000);

            _stop = true;
            Console.WriteLine("Stop 호출");

            Console.WriteLine("종료 대기중");

            task.Wait();    // task의 join

            Console.WriteLine("종료 성공");
        }
    }
}
