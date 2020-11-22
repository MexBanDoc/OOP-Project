namespace Mafia.Domain
{
    public interface IInteraction
    {
        void Interact(IPerson person);
        PersonState ResultTargetState { get; }
    }
}