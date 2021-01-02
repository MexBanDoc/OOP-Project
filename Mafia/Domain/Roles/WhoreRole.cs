using System.Collections.Generic;

namespace Mafia.Domain
{
    public class WhoreRole : Role
    {
        public WhoreRole() : base(new List<IInteraction> {new HealInteraction(), new CrazyInteraction()})
        {
        }

        public override string Name { get; } = "НоЧнАя БаБоЧкА";
        public override DayTime DayTime { get; } = DayTime.Night;
    }
}