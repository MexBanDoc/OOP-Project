using System.Collections.Generic;

namespace Mafia.Domain
{
    public class DoctorRole : Role
    {
        public DoctorRole() : base(new List<IInteraction>{new HealInteraction()})
        {
        }

        public override string Name { get; } = "D♀ct♂r";
        public override DayTime DayTime { get; } = DayTime.Night;
    }
}