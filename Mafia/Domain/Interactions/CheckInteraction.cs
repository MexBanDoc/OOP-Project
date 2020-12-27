namespace Mafia.Domain
{
    public class CheckInteraction : IInteraction
    {
        
        public PersonState Interact(IPerson person)
        {
            return person.NightRole.Equals(new MafiaRole()) ? PersonState.Killed : PersonState.Alive;
        }
    }
}