namespace Mafia.Domain
{
    public class CheckInteraction : IInteraction
    {
        private PersonState lastState = PersonState.Alive;
        
        public void Interact(IPerson person)
        {
            lastState = person.NightRole.Equals(new MafiaRole()) ? PersonState.Killed : PersonState.Alive;
        }

        public PersonState ResultTargetState => lastState;
    }
}