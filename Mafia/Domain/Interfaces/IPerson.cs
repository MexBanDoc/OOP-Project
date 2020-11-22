using System.Collections.Generic;

namespace Mafia.Domain
{
    public interface IPerson
    {
        public bool TryKill();
        public void Heal();
        public bool IsAlive { get; }
        public bool IsImmortal { get; set; }
        public Role DayRole { get; }
        public Role NightRole { get; }
        public string Name { get; }
    }
}