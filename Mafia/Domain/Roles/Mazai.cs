using System.Collections.Generic;

namespace Mafia.Domain
{
    public class Mazai : Role
    {
        public Mazai() : base(new List<IInteraction> {new SavingRabbitsInteraction()})
        {
        }

        public override string Name { get; } = "Ded Mazai";
        public override DayTime DayTime { get; } = DayTime.Night;
    }
}