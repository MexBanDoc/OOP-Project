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
        
        public bool IsOpen { get; private set; } = true;  // TODO: remove
        
        private readonly Random random = new Random();
        private readonly ConcurrentDictionary<long, string> players = new ConcurrentDictionary<long, string>
        {
            [540232512] = "Timofey Belov (@MexBanDoc)",
            [626404561] = "Danya Krutovsky (@krutovsky)",
            [376240791] = "ЛЕНА (@mefoolyhi)",
            [756835435] = "Коля @prefie",
            [527302283] = "Дима @bro_en",
            [749270491] = "СТЕПА @skachusov",
            [930202628] = "Android @Andrey135296"
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

        public IEnumerable<(long, IPerson)> ExtractPersons(ISettings settings)
        {
            IsOpen = false;
            
            var ids = new long[players.Count];
            var names = new string[players.Count];
            var i = 0;

            foreach (var player in players)
            {
                ids[i] = player.Key;
                names[i++] = player.Value;
            }

            i = 0;

            foreach (var person in settings.GeneratePopulation(names, random))
            {
                yield return (ids[i++], person);
            }
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