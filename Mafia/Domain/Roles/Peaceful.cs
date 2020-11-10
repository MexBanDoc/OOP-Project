namespace Mafia.Domain
{
    public class Peaceful : Role
    {
        public override void Interact(IPerson target)
        {
            return;
        }

        public override string Name { get; } = "Peaceful";
    }
}