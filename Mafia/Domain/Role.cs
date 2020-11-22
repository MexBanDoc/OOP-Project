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
                return Name.Equals(role.Name);
            return Equals(obj, this);
        }

        public abstract string Name { get; }
        public abstract DayTime dayTime { get; }
    }
}