using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Mafia.Domain;

namespace Mafia.App
{
    public class PlayersPool : IPlayersPool
    {
        private static readonly List<string> Names = new List<string>{
            "Liam", "Olivia", "Noah", "Emma",
            "Oliver", "Ava", "William", "Sophia",
            "Elijah", "Isabella", "James", "Charlotte",
            "Benjamin", "Amelia", "Lucas", "Mia",
            "Mason", "Harper", "Ethan", "Evelyn"
        };
        
        public bool IsOpen { get; private set; } = true;
        
        private readonly Random random = new Random();
        private readonly ConcurrentDictionary<long, IPerson> players = new ConcurrentDictionary<long, IPerson>();

        public bool AddPlayerAsync(long playerId)
        {
            if (players.ContainsKey(playerId))
            {
                return false;
            }

            var person = CreatePerson();
            players[playerId] = person;
            return true;
        }

        private IPerson CreatePerson()
        {
            var dayRole = new CitizenRole();
            var index = players.Count % 3; // count of inheritors of Role
            Role nightRole = index switch // TODO: make this cool
            {
                1 => new MafiaRole(),
                2 => new HealerRole(),
                _ => null
            };

            var name = Names[random.Next(0, Names.Count)];
            Names.Remove(name);
            
            return new Person(dayRole, nightRole, name);
        }

        public IEnumerable<KeyValuePair<long, IPerson>> ExtractPersons()
        {
            IsOpen = false;
            return players;
        }
    }
}