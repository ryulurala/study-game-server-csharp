using System;
using System.Threading;
using System.Threading.Tasks;

namespace test
{
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
    class Program
    {
        static void Main(string[] args)
        {

        }
    }
}
