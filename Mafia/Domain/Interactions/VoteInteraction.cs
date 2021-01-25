namespace Mafia.Domain
{
    public class VoteInteraction : IInteraction
    {
        public PersonState Interact(IPerson person) => PersonState.Killed;
    }
}