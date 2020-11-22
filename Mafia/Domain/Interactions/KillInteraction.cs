﻿namespace Mafia.Domain
{
    public class KillInteraction : IInteraction
    {
        public void Interact(IPerson person)
        {
            person.TryKill();
        }

        public PersonState ResultTargetState => PersonState.Killed;
    }
}