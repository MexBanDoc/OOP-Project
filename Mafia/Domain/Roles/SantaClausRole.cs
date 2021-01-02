using System.Collections.Generic;

namespace Mafia.Domain
{
    public class SantaClausRole : Role

    {
        public SantaClausRole() : base(new List<IInteraction> {new XmasInteraction()})
        {
        }

        public override string Name { get; } = "Ded Moroz";
        public override DayTime DayTime { get; } = DayTime.Night;
    }
}