namespace Mafia.Domain
{
    public class SavingRabbitsInteraction : IInteraction
    {
        public PersonState Interact(IPerson person)
        {
            var name = person.Name;
            name = name.Replace(" ", " 🐇 ");
            person.Name = name.Insert(name.Length, "🛶🐰");
            return PersonState.Alive;
        }
    }
}