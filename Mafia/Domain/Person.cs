using System.Collections.Generic;

namespace Mafia.Domain
{
    public class Person : IPerson
    {
        public Person(Role dayRole, Role nightRole)
        {
            DayRole = dayRole;
            NightRole = nightRole;
        }
        
        public void TryKill()
        {
            if (!IsImmortal)
                IsAlive = false;
        }

        public void Heal()
        {
            IsAlive = true;
            IsImmortal = true;
        }

        public bool IsAlive { get; private set; } = true;
        public bool IsImmortal { get; set; } = false;
        public Role DayRole { get; }
        public Role NightRole { get; }

        public string Name { get; }
    }
}