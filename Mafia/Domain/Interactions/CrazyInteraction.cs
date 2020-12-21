using System.Text;

namespace Mafia.Domain
{
    public class CrazyInteraction : IInteraction
    {
        public void Interact(IPerson person)
        {
            var name = new StringBuilder();
            for (var i = 0; i < person.Name.Length; i++)
                name.Append(i % 2 == 0 ? char.ToUpper(person.Name[i]) : char.ToLower(person.Name[i]));
            person.Name = name.ToString();
        }
        public PersonState ResultTargetState { get; } = PersonState.Immortal;
    }
}