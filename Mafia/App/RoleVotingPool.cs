using System.Collections.Concurrent;
using System.Threading;

namespace Mafia.App
{
    public class RoleVotingPool
    {
        private readonly string[] names;
        private readonly int[] voteCount;
        private readonly Semaphore votingSemaphore = new Semaphore(1, 1);
        private readonly ConcurrentDictionary<long, int> targets = new ConcurrentDictionary<long, int>();

        public RoleVotingPool(string[] names)
        {
            this.names = names;
            voteCount = new int[names.Length];
        }
        
        public string Vote(long userId, int name)
        {
            
            if (name >= names.Length)
            {
                return "Not allowed name :(";
            }
            
            var result = targets.ContainsKey(userId)
                ? "Successfully rewrite target 😏"
                : "Successfully chose target 😱";

            var lockTaken = false;

            try
            {
                votingSemaphore.WaitOne();
                lockTaken = true;
                
                voteCount[name]++;

                if (targets.ContainsKey(userId))
                {
                    voteCount[targets[userId]]--;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    votingSemaphore.Release();
                }
            }

            targets[userId] = name;
            return result;
        }

        public string GetWinner()
        {
            var index = 0;
            var maximum = 0;

            var lockTaken = false;

            try
            {
                votingSemaphore.WaitOne();
                lockTaken = true;
                
                for (var i = 0; i < names.Length; i++)
                {
                    if (voteCount[i] <= maximum) continue;
                    index = i;
                    maximum = voteCount[i];
                }
            }
            finally
            {
                if (lockTaken)
                {
                    votingSemaphore.Release();
                }
            }

            return names[index];
        }
    }
}