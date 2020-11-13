using System.Collections.Generic;

namespace Mafia.Domain
{
    public class Healer : Role
    {
        public Healer() : base(new List<IInteraction>{new HealInteraction()})
        {
        }

        public override string Name { get; }
        public override DayTime dayTime { get; } = DayTime.Night;
    }
}