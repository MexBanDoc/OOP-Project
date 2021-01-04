using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Mafia.Domain
{
    public interface ICity
    {
        public string Name { get; }
        public ICollection<IPerson> Population { get; }
        public ICollection<Role> Roles { get; }
        public DayTime DayTime { get; }
        public ConcurrentDictionary<IPerson, PersonState> LastChanges { get; }

        public IPerson GetPersonByName(string name);

        public void StartNight();
        public void StartDay();

        public void AddChange(IPerson target, PersonState state);
    }
}