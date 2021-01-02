using System;
using System.Collections.Generic;
using System.Linq;
using Mafia.Infrastructure;

namespace Mafia.Domain
{
    public class Settings : ISettings
    {
        public Func<ICity, WinState> WinCondition { get; }
        public string CityName { get; } = "CumCockCity";
        public Dictionary<Role, int> PlayerDistribution { get; }

        public Settings(Func<ICity, WinState> winCondition, Dictionary<Role, int> playerDistribution)
        {
            WinCondition = winCondition;
            PlayerDistribution = playerDistribution;
        }

        public IEnumerable<IPerson> GeneratePopulation(string[] names, Random random)
        {
            var result = new IPerson[names.Length];

            var indexes = Enumerable.Range(0, names.Length)
                .OrderBy(x => random.Next()).ToList();

            var currentIndex = 0;
            var citizen = new CitizenRole();

            foreach (var role in PlayerDistribution.Keys)
            {
                var playersCount = Math.Max(PlayerDistribution[role] * names.Length / 100, 1);
                var upper = Math.Min(currentIndex + playersCount, names.Length);
                for (var i = currentIndex; i < upper; i++)
                {
                    result[indexes[i]] = new Person(citizen, role, names[indexes[i]]);
                }

                currentIndex += playersCount;
            }

            for (; currentIndex < indexes.Count; currentIndex++)
            {
                result[indexes[currentIndex]] = new Person(citizen, null, names[indexes[currentIndex]]);
            }

            return result;
        }

        public static WinState DefaultWinCondition(ICity city)
        {
            var mafiaCount = city.Population.Count(p => p.NightRole is MafiaRole && p.IsAlive);
            var totalCount = city.Population.Count(p => p.IsAlive);
            if (mafiaCount == 0)
                return WinState.PeacefulWins;
            if (totalCount >= 1 && totalCount - mafiaCount <= 1)
                return WinState.MafiaWins;
            return WinState.InProcess;
        }
        
        public static readonly Settings Default = new Settings(DefaultWinCondition, 
            new Dictionary<Role, int> {[new MafiaRole()] = 20});
    }
    
    // TODO: at least one of every role
    // TODO: up count to percentage
}