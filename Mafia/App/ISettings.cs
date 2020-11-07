using System;
using Mafia.Domain;

namespace Mafia.App
{
    public interface ISettings
    {
        public Func<ICity, WinState> WinCondition { get; }
        
        public int Players { get; }
        
        public bool EnableWhores { get; }
        
        public bool EnableManiacs { get; }
        
        
    }
}