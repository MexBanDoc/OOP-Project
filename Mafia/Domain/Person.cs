namespace Mafia.Domain
{
    public class Person : IPerson
    {
        public Person(Role role)
        {
            Role = role;
        }
        
        public void Die()
        {
            if (!IsImmortal)
                IsAlive = false;
        }

        public bool IsAlive { get; private set; } = true;
        public bool IsImmortal { get; set; } = false;
        public Role Role { get; }
    }
}