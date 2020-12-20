using System.Collections.Generic;
using System.Linq;

namespace Mafia.Domain
{
    public abstract class Role : IInteraction
    {
        private readonly List<IInteraction> interactions;

        public PersonState ResultTargetState => interactions.Select(inter => inter.ResultTargetState).Max();
        public Role(List<IInteraction> interactions)
        {
            this.interactions = interactions;
        }

        public void Interact(IPerson person)
        {
            foreach (var interaction in interactions)
            {
                interaction.Interact(person);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Role role)
                return Equals(role);
            return Equals(obj, this);
        }

        private bool Equals(Role other)
        {
            return Equals(interactions, other.interactions) && Name == other.Name && DayTime == other.DayTime;
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