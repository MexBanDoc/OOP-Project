namespace Mafia.Domain
{
    public class HealInteraction : IInteraction
    {
        public PersonState Interact(IPerson person)
        {
            person.Heal();
            return PersonState.Immortal;
        }
    }
}