using System.Collections.Generic;

namespace Mafia.Domain
{
    public class Peaceful : Role
    {
        public Peaceful() : base(new List<IInteraction>(new[] {new VoteInteraction()}))
        {
        }

        public override string Name { get; } = "Мирный житель";
    }
}