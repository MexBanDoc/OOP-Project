using System.Collections.Generic;

namespace Mafia.Domain
{
    public class Citizen : Role
    {
        public Citizen() : base(new List<IInteraction>{new VoteInteraction()})
        {
        }

        public override string Name { get; } = "Гражданин";
        public override DayTime dayTime { get; } = DayTime.Day;
    }
}