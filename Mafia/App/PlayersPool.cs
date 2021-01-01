using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mafia.Domain;

namespace Mafia.App
{
    public class PlayersPool : IPlayersPool
    {
        // private static readonly List<string> Names = new List<string>{
        //     "Liam", "Olivia", "Noah", "Emma",
        //     "Oliver", "Ava", "William", "Sophia",
        //     "Elijah", "Isabella", "James", "Charlotte",
        //     "Benjamin", "Amelia", "Lucas", "Mia",
        //     "Mason", "Harper", "Ethan", "Evelyn"
        // };
        
        public bool IsOpen { get; private set; } = true;
        
        private readonly Random random = new Random();
        private readonly ConcurrentDictionary<long, string> players = new ConcurrentDictionary<long, string>
        {
            [540232512] = "/MexBanDoc",
            [626404561] = "Danya Krutovsky (@krutovsky)",
            [376240791] = "/mefoolyhi",
            [756835435] = "prefie",
            [527302283] = "bro_en",
            [749270491] = "skachusov",
            [930202628] = "Andrey135296"
        };

        public bool AddPlayer(long playerId, string name)
        {
            if (players.ContainsKey(playerId))
            {
                return false;
            }
            
            players[playerId] = name;
            return true;
        }

        public IEnumerable<(long, IPerson)> ExtractPersons()
        {
            IsOpen = false;
            var pool = players.ToList();

            return ForMethod(new MafiaRole(), 4, pool)
                .Concat(ForMethod(new PoliсemanRole(), 6, pool))
                .Concat(ForMethod(new HealerRole(), 5, pool))
                // .Concat(ForMethod(new WhoreRole(), 4, pool))
                .Concat(ForMethod(new SantaClausRole(), 3, pool))
                .Concat(ForMethod(null, 1, pool));
        }

        private IEnumerable<(long, IPerson)> ForMethod(Role role, int part, IList<KeyValuePair<long, string>> pool)
        {
            for (var i = 0; i < (players.Count + part - 1) / part; i++)
            {
                var index = Math.Max(0, random.Next(pool.Count) - 1);
                if (index >= pool.Count)
                {
                    continue;
                }
                var id = pool[index].Key;
                var name = pool[index].Value;
                pool.RemoveAt(index);
                yield return (id, new Person(new CitizenRole(), role, name));
            }
        }
    }
}