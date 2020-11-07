namespace Mafia.Domain
{
    public interface IPerson
    {
        public void Die();
        public bool IsAlive { get; }
    }
}