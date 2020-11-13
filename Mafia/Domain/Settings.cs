using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mafia.Infrastructure;

namespace Mafia.Domain
{
    public class Settings : ISettings
    {
        public Func<ICity, WinState> WinCondition { get; }
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
            var dayDistribution = PlayerDistribution.Where(t => t.Item1.dayTime == DayTime.Day);
            var nightDistribution = PlayerDistribution.Where(t => t.Item1.dayTime == DayTime.Night);
            var dayRoles = dayDistribution.Multiply().ToList();
            var nightRoles = nightDistribution.Multiply().ToList();
            for (int i = 0; i < TotalPlayers; i++)
                yield return new Person(dayRoles.Count>i?dayRoles[i]:null,
                    nightRoles.Count>i?nightRoles[i]:null);
        }
        
        public static Settings Default = new Settings(
            (city) =>
                {
                    var mafiaCount = city.Population.Count(p => p.NightRole is MafiaRole && p.IsAlive);
                    var totalCount = city.Population.Count(p => p.IsAlive);
                    if (mafiaCount == 0)
                        return WinState.MafiaWins;
                    if (totalCount - mafiaCount < mafiaCount)
                        return WinState.MafiaWins;
                    return WinState.InProcess;
                },
            new List<Tuple<Role, int>>{Tuple.Create((Role)new MafiaRole(), 1), Tuple.Create((Role)new CitizenRole(), 4)},
            4);
    }
}