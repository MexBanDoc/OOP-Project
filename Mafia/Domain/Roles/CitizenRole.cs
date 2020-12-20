using System.Collections.Generic;

namespace Mafia.Domain
{
    public class CitizenRole : Role
    {
        public CitizenRole() : base(new List<IInteraction>{new VoteInteraction()})
        {
        }

        public override string Name { get; } = "Peaceful";
        public override DayTime DayTime { get; } = DayTime.Day;
    }
}