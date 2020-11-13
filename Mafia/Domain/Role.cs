using System.Collections.Generic;

namespace Mafia.Domain
{
    public abstract class Role : IInteraction
    {
        private readonly List<IInteraction> interactions;

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
        public abstract string Name { get; }
        public abstract DayTime dayTime { get; }
    }
}