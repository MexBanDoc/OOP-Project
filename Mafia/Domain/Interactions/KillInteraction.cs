namespace Mafia.Domain
{
    public class KillInteraction : IInteraction
    {
        public void Interact(IPerson person)
        {
            
        }

        public PersonState ResultTargetState => PersonState.Killed;
    }
}