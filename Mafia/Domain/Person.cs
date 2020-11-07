namespace Mafia.Domain
{
    public class Person : IPerson
    {
        public Person(Role role)
        {
            IsAlive = true;
            Role = role;
        }
        
        public void Die()
        {
            IsAlive = false;
        }

        public bool IsAlive { get; private set; }
        public Role Role { get; }
    }
}