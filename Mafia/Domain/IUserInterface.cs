using System.Collections.Generic;
using Mafia.Domain;

namespace Mafia.Domain
{
    public interface IUserInterface
    {
        public IPerson AskForInteractionTarget(IEnumerable<IPerson> players, Role role);

        public void StarGame();
        
        // allow players make random choice
    }
}