using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace Mafia.Domain
{
    public class City : ICity
    {
        public ICollection<IPerson> Population { get; }
        public ICollection<Role> Roles { get; } = new List<Role>();
        public DayTime DayTime { get; private set; }
        
        public City(ICollection<IPerson> population)
        {
            Population = population;
            DayTime = DayTime.Day;
        }
        
        public void StartNight()
        {
            DayTime = DayTime.Night;
        }

        public void StartDay()
        {
            foreach (var person in Population)
            {
                person.IsImmortal = false;
            }
            DayTime = DayTime.Day;
        }

        public void AddRole(Role role)
        {
            Roles.Add(role);
        }
    }
}