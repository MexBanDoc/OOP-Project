using System;
using System.Collections.Generic;

namespace Mafia.Domain
{
    public interface ISettings
    {
        public int VoteDelay { get; }
        public Func<ICity, WinState> WinCondition { get; }
        
        public string CityName { get; }
        
        public Dictionary<Role, int> PlayerDistribution { get; }

        public IEnumerable<IPerson> GeneratePopulation(string[] names, Random random);
    }
}