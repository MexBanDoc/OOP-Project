using System.Collections.Generic;
using System.Linq;

namespace Mafia.Domain
{
    public class City : ICity
    {
        public ICollection<IPerson> Population { get; }
        public ICollection<Role> Roles { get; } = new HashSet<Role>();
        public DayTime DayTime { get; private set; }
        
        public Dictionary<IPerson, PersonState> LastChanges { get; private set; }
        
        public City(ICollection<IPerson> population)
        {
            Population = population;
            LastChanges = new Dictionary<IPerson, PersonState>();
            foreach (var person in population)
            {
                if (!(person.NightRole is null))
                    Roles.Add(person.NightRole);
                Roles.Add(person.DayRole);
            }
            DayTime = DayTime.Day;
        }

        public IPerson GetPersonByName(string name)
        {
            return Population.First(person => person.Name.Equals(name));
        }

        public void StartNight()
        {
            LastChanges = new Dictionary<IPerson, PersonState>();
            DayTime = DayTime.Night;
        }

        public void StartDay()
        {
            LastChanges = new Dictionary<IPerson, PersonState>();
            foreach (var person in Population)
            {
                person.IsImmortal = false;
            }
            DayTime = DayTime.Day;
        }

        public void AddChange(IPerson target, Role role)
        {
            LastChanges.Add(target, role.ResultTargetState);
        }
    }
}