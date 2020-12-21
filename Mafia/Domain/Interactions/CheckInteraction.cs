namespace Mafia.Domain
{
    public class CheckInteraction : IInteraction
    {
        public void Interact(IPerson person)
        {
            throw new System.NotImplementedException();
        }

        public PersonState ResultTargetState { get; }
    }
}