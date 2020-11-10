namespace Mafia.Domain
{
    public abstract class Role
    {
        public abstract void Interact(IPerson person);
        public abstract string Name { get; }
    }
}