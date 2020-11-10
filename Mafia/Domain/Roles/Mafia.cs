namespace Mafia.Domain
{
    public class Mafia : Role
    {
        public override void Interact(IPerson target)
        {
            target.Die();
        }

        public override string Name { get; } = "Mafia";
    }
}