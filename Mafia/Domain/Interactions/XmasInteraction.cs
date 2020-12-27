namespace Mafia.Domain
{
    public class XmasInteraction : IInteraction
    {
        public PersonState Interact(IPerson person)
        {
            var name = person.Name;
            name = name.Replace(" ", " 🎄 ");
            person.Name = name.Insert(name.Length, "🍾🎁");
            return PersonState.Alive;
        }

        public PersonState ResultTargetState { get; } = PersonState.Alive;
    }
}