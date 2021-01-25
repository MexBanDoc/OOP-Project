namespace Mafia.Domain
{
    public class KillInteraction : IInteraction
    {
        public PersonState Interact(IPerson person) => PersonState.Killed;
    }
}