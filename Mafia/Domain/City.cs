using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace Mafia.Domain
{
    public class City : ICity
    {
        public ICollection<IPerson> Population { get; }
        public ICollection<IPerson> Immortals { get; private set; }
        public DayTime DayTime { get; private set; }
        
        public City(ICollection<IPerson> population)
        {
            Population = population;
            DayTime = DayTime.Day;
        }
        
        public void EndDay()
        {
            DayTime = DayTime.Night;
        }

        public void EndNight()
        {
            Immortals.Clear();
            DayTime = DayTime.Day;
        }

        public void Kill(IPerson person)
        {
            if (!Population.Contains(person))
            {
                Console.Error.Write("Unknown Person");
                return;
            }

            if (!Immortals.Contains(person))
                person.Die();
        }
    }
}