using System.Collections.Generic;

namespace Mafia.Domain
{
    public interface ICity
    {
        public ICollection<IPerson> Population { get; }
        public DayTime DayTime { get; }
    }
}