using System.Collections.Generic;

namespace Mafia.Domain
{
    public interface ICity
    {
        public ICollection<IPerson> Population { get; }
        public DayTime DayTime { get; }

        public void StartNight();
        public void StartDay();

        public void Heal(IPerson person);

        public void Murder(IPerson person);

        public Role Investigate(IPerson person);
    }
}