namespace Mafia.Domain
{
    public class XmasInteraction : IInteraction
    {
        public void Interact(IPerson person)
        {
            var name = person.Name;
            person.Name = name.Replace(" ", "🎄").Insert(name.Length + 2, "🍾🎁");
        }

        public PersonState ResultTargetState { get; } = PersonState.Alive;
    }
}