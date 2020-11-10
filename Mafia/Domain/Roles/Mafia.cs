using System.Collections.Generic;

namespace Mafia.Domain
{
    public class Mafia : Role
    {
        public Mafia() : base(new List<IInteraction>(new IInteraction[] {new KillInteraction(), new VoteInteraction()}))
        {
        }

        public override string Name { get; } = "Мафия";
    }
}