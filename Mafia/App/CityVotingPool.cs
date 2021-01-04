using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Mafia.Domain;

namespace Mafia.App
{
    public class CityVotingPool
    {
        private readonly Semaphore allowedChatsSemaphore = new Semaphore(1, 1);
        private readonly HashSet<long> allowedChats = new HashSet<long>();
        private readonly ConcurrentDictionary<long, Role> userToRole = new ConcurrentDictionary<long, Role>();
        private readonly ConcurrentDictionary<Role, RoleVotingPool> roleVotePools = new ConcurrentDictionary<Role, RoleVotingPool>();

        public void AddChatId(long chatId)
        {
            var lockTaken = false;

            try
            {
                allowedChatsSemaphore.WaitOne();
                lockTaken = true;

                allowedChats.Add(chatId);
            }
            finally
            {
                if (lockTaken)
                {
                    allowedChatsSemaphore.Release();
                }
            }
        }

        public void AddRole(long userId, Role role)
        {
            userToRole[userId] = role;
        }

        public void AddRoleVoting(Role role, RoleVotingPool votingPool)
        {
            roleVotePools[role] = votingPool;
        }

        public string Vote(long chatId, long userId, int name)
        {
            var lockTaken = false;
            bool chatIsAllowed;

            try
            {
                allowedChatsSemaphore.WaitOne();
                lockTaken = true;

                chatIsAllowed = allowedChats.Contains(chatId);
            }
            finally
            {
                if (lockTaken)
                {
                    allowedChatsSemaphore.Release();
                }
            }

            if (!chatIsAllowed)
            {
                return "Not allowed vote there! 😡";
            }

            if (!userToRole.ContainsKey(userId))
            {
                return "Who are you? 🧐";
            }
            
            var role = userToRole[userId];

            if (!roleVotePools.ContainsKey(role))
            {
                return "You cannot vote 😞";
            }

            return roleVotePools[role].Vote(userId, name);
        }

        public string ExtractRoleVoteWinner(Role role)
        {
            return roleVotePools.TryRemove(role, out var pool) ? pool.GetWinner() : null;
        }

        public bool IsEmpty => roleVotePools.IsEmpty;
    }
}