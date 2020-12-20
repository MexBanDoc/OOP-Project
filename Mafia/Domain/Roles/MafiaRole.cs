using System.Collections.Generic;

namespace Mafia.Domain
{
    public class MafiaRole : Role
    {
        public MafiaRole() : base(new List<IInteraction>{new KillInteraction()})
        {
        }

        public override string Name { get; } = "Мафия";
        public override DayTime DayTime { get; } = DayTime.Night;
    }
}