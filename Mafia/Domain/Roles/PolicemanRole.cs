using System.Collections.Generic;

namespace Mafia.Domain
{
    public class PoliсemanRole : Role
    {
        public PoliсemanRole() : base(new List<IInteraction>() {new CheckInteraction()})
        {
        }

        public override string Name { get; } = "Policeman";
        public override DayTime DayTime { get; } = DayTime.Night;
    }
}