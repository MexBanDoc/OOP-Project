namespace Mafia.Domain
{
    public class CheckInteraction : IInteraction
    {
        public void Interact(IPerson person)
        {
            if (person.NightRole.Equals(new MafiaRole()))
            {
                person.TryKill();
            }
        }

        public PersonState ResultTargetState => PersonState.Alive;
    }
}