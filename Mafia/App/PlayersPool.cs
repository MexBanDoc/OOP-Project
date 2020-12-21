using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private readonly ConcurrentDictionary<long, IPerson> players = new ConcurrentDictionary<long, IPerson>();

        public bool AddPlayer(long playerId, string name)
        {
            if (players.ContainsKey(playerId))
            {
                return false;
            }

            var person = CreatePerson(name);
            players[playerId] = person;
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

        public IEnumerable<KeyValuePair<long, IPerson>> ExtractPersons()
        {
            IsOpen = false;
            // TODO: choose roles cool way
            return players;
        }
    }
}