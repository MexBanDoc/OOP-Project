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
        private readonly ConcurrentDictionary<long, string> players = new ConcurrentDictionary<long, string>();

        public bool AddPlayer(long playerId, string name)
        {
            if (players.ContainsKey(playerId))
            {
                return false;
            }
            
            players[playerId] = name;
            return true;
        }

        private IPerson CreatePerson(string name)
        {
            var dayRole = new CitizenRole();
            var index = players.Count % 3; // count of inheritors of Role
            // index = random.Next(2);
            Role nightRole = index switch
            {
                1 => new MafiaRole(),
                2 => new HealerRole(),
                _ => null
            };

            // var name = Names[random.Next(0, Names.Count)];
            // Names.Remove(name);
            
            return new Person(dayRole, nightRole, name);
        }

        public IEnumerable<(long, IPerson)> ExtractPersons()
        {
            IsOpen = false;
            var pool = players.ToList();

            return ForMethod(new MafiaRole(), 4, pool)
                .Concat(ForMethod(new HealerRole(), 6, pool))
                .Concat(ForMethod(null, 1, pool));
        }

        private IEnumerable<(long, IPerson)> ForMethod(Role role, int part, IList<KeyValuePair<long, string>> pool)
        {
            for (var i = 0; i < players.Count / part; i++)
            {
                var index = Math.Max(0, random.Next(players.Count) - 1);
                var id = pool[index].Key;
                var name = pool[index].Value;
                pool.RemoveAt(index);
                yield return (id, new Person(new CitizenRole(), role, name));
            }
        }
    }
}