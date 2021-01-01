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
        public List<Tuple<Role, int>> PlayerDistribution { get; }
        public int TotalPlayers { get; }

        public Settings(Func<ICity, WinState> winCondition, List<Tuple<Role, int>> playerDistribution, int totalPlayers)
        {
            WinCondition = winCondition;
            PlayerDistribution = playerDistribution;
            TotalPlayers = totalPlayers;
        }

        public IEnumerable<IPerson> GeneratePopulation()
        {
            var dayDistribution = PlayerDistribution.Where(t => t.Item1.DayTime == DayTime.Day);
            var nightDistribution = PlayerDistribution.Where(t => t.Item1.DayTime == DayTime.Night);
            var dayRoles = dayDistribution.Multiply().ToList();
            var nightRoles = nightDistribution.Multiply().ToList();
            for (int i = 0; i < TotalPlayers; i++)
                yield return new Person(dayRoles.Count > i ? dayRoles[i] : null,
                    nightRoles.Count > i ? nightRoles[i] : null, $"Person{i}");
        }

        public static readonly Func<ICity, WinState> DefaultWinCondition = (city) =>
        {
            var mafiaCount = city.Population.Count(p => p.NightRole is MafiaRole && p.IsAlive);
            var totalCount = city.Population.Count(p => p.IsAlive);
            if (mafiaCount == 0)
                return WinState.PeacefulWins;
            if (totalCount >= 1 && totalCount - mafiaCount <= 1)
                return WinState.MafiaWins;
            return WinState.InProcess;
        };
        
        public static readonly Settings Default = new Settings(DefaultWinCondition,
            new List<Tuple<Role, int>>
                {Tuple.Create((Role) new MafiaRole(), 1), Tuple.Create((Role) new CitizenRole(), 4)},
            4);
    }
}