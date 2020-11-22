namespace Mafia.Domain
{
    public class HealInteraction : IInteraction
    {
        public void Interact(IPerson person)
        {
            person.Heal();
        }

        public PersonState ResultTargetState => PersonState.Immortal;
    }
}