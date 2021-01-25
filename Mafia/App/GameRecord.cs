using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Mafia.Domain;

namespace Mafia.App
{
    public class GameRecord : IGameRecord
    {

        private readonly Random random;
        private readonly ConcurrentDictionary<long, string> players = new ConcurrentDictionary<long, string>();

        private ISettings settings = Domain.Settings.Various;

        public GameRecord(Random random)
        {
            this.random = random;
        }

        public ISettings Settings
        {
            get => settings;
            set
            {
                if (value != null) settings = value;
            }
        }

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
            var ids = new long[players.Count];
            var names = new string[players.Count];
            var i = 0;

            foreach (var player in players)
            {
                ids[i] = player.Key;
                names[i++] = player.Value;
            }

            i = 0;

            foreach (var person in Settings.GeneratePopulation(names, random))
            {
                yield return (ids[i++], person);
            }
        }
    }
}