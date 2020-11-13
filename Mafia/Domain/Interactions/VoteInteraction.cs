namespace Mafia.Domain
{
    public class VoteInteraction : IInteraction
    {
        public void Interact(IPerson person)
        {
            person.Die();
        }
    }
}