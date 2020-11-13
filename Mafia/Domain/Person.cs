namespace Mafia.Domain
{
    public class Person : IPerson
    {
        public Person(Role role)
        {
            Role = role;
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
        public Role Role { get; }
        public string Name { get; }
    }
}