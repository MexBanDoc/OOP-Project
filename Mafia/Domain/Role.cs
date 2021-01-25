using System;
using System.Collections.Generic;
using System.Linq;

namespace Mafia.Domain
{
    public abstract class Role : IInteraction
    {
        private readonly List<IInteraction> interactions;

        public Role(List<IInteraction> interactions)
        {
            this.interactions = interactions;
        }

        public PersonState Interact(IPerson person)
        {
            return interactions.Select(inter => inter.Interact(person)).Max();
        }

        public override bool Equals(object obj)
        {
            if (obj is Role role)
                return Equals(role);
            return Equals(obj, this);
        }

        private bool Equals(Role other)
        {
            return Name == other.Name && DayTime == other.DayTime;
        }
        
        public static bool operator ==(Role first, Role second)
        {
            object other = second;
            if ((object) first == null)
                return other == null;
            return first.Equals(other);
        }

        public static bool operator !=(Role role, Role obj)
        {
            return !(role == obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = interactions != null ? interactions.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) DayTime;
                return hashCode;
            }
        }

        public abstract string Name { get; }
        public abstract DayTime DayTime { get; }
    }
}