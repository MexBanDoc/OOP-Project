namespace Mafia.Domain
{
    public class XmasInteraction : IInteraction
    {
        public void Interact(IPerson person)
        {
            var name = person.Name;
            name = name.Replace(" ", "🎄 ");
            person.Name = name.Insert(name.Length - 1, "🍾🎁");
        }

        public PersonState ResultTargetState { get; } = PersonState.Alive;
    }
}