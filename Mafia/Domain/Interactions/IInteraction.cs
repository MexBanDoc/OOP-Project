namespace Mafia.Domain
{
    public interface IInteraction
    {
        PersonState Interact(IPerson person);
    }
}