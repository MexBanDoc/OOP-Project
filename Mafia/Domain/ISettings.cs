using System;
using System.Collections.Generic;
using Mafia.Domain;

namespace Mafia.Domain
{
    public interface ISettings
    {
        public Func<ICity, WinState> WinCondition { get; }

        public List<Tuple<Role, int>> PlayerDistribution { get; }

        public int Players { get; }
        
        public bool EnableWhores { get; }
        
        public bool EnableManiacs { get; }
    }
}