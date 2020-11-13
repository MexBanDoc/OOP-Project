using System.Collections.Generic;

namespace Mafia.Domain
{
    public class HealerRole : Role
    {
        public HealerRole() : base(new List<IInteraction>{new HealInteraction()})
        {
        }

        public override string Name { get; }
        public override DayTime dayTime { get; } = DayTime.Night;
    }
}