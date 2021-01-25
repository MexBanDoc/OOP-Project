namespace Mafia.Domain
{
    public class Person : IPerson
    {
        public Person(Role dayRole, Role nightRole, string name)
        {
            DayRole = dayRole;
            NightRole = nightRole;
            Name = name;
        }

        public Person()
        {
        }

        public bool TryKill()
        {
            if (IsImmortal) return false;
            IsAlive = false;
            return true;

        }

        public void Heal()
        {
            IsAlive = true;
            IsImmortal = true;
        }

        public bool IsAlive { get; private set; } = true;
        public bool IsImmortal { get; set; }
        public Role DayRole { get; }
        public Role NightRole { get; }

        public string Name { get; set; }
    }
}