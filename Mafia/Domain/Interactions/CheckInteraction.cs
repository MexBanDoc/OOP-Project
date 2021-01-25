namespace Mafia.Domain
{
    public class CheckInteraction : IInteraction
    {
        public PersonState Interact(IPerson person)
        {
            if (person.NightRole == null) return PersonState.Alive;
            return person.NightRole.Equals(new MafiaRole()) ? PersonState.Killed : PersonState.Alive;
        }
    }
}