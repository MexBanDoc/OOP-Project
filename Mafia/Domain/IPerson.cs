namespace Mafia.Domain
{
    public interface IPerson
    {
        public void TryKill();
        public void Heal();
        public bool IsAlive { get; }
        public bool IsImmortal { get; set; }
        public Role Role { get; }
        public string Name { get; }
    }
}