﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Mafia.Domain
{
    public class City : ICity
    {
        private static readonly ConcurrentBag<string> NamesPool = new ConcurrentBag<string>
        {
            "Tamriel",
            "New_Vegas",
            "Dirthmous",
            "FireLink"
        };
        
        public string Name { get; }
        public ICollection<IPerson> Population { get; }
        public ICollection<Role> Roles { get; } = new HashSet<Role>();
        public DayTime DayTime { get; private set; }
        public ConcurrentDictionary<IPerson, PersonState> LastChanges { get; private set; }
        
        public City(ICollection<IPerson> population, string cityName)
        {
            Population = population;
            Name = cityName;
            if (NamesPool.TryTake(out cityName))
            {
                // poor trick
                Name = cityName;
            }
            LastChanges = new ConcurrentDictionary<IPerson, PersonState>();
            foreach (var person in population)
            {
                if (!(person.NightRole is null))
                    Roles.Add(person.NightRole);
                Roles.Add(person.DayRole);
            }
            DayTime = DayTime.Day;
        }

        public IPerson GetPersonByName(string name)
            => Population.FirstOrDefault(person => person.Name.Equals(name));
        

        public void StartNight()
        {
            LastChanges = new ConcurrentDictionary<IPerson, PersonState>();
            DayTime = DayTime.Night;
        }

        public void StartDay()
        {
            LastChanges = new ConcurrentDictionary<IPerson, PersonState>();
            foreach (var person in Population)
            {
                person.IsImmortal = false;
            }
            DayTime = DayTime.Day;
        }

        public void AddChange(IPerson target, PersonState state)
        {
            if (!LastChanges.ContainsKey(target))
            {
                LastChanges[target] = state;
                return;
            }
            if (LastChanges[target] != PersonState.Immortal)
            {
                LastChanges[target] = state;
            }
        }

        ~City()
        {
            NamesPool.Add(Name);
        }
    }
}