using System.Collections.Generic;
using Mafia.Domain;

namespace Mafia.Domain
{
    public interface IUserInterface
    {
        public IPerson GetPersonToHeal(IEnumerable<IPerson> doctor);

        public IPerson GetPersonToMurder(IEnumerable<IPerson> mafia);

        public IPerson GetPersonToInvestigate(IEnumerable<IPerson> sheriff);

        public IPerson AskForTarget(IEnumerable<IPerson> players, Role role);

        public void StarGame();
        
        // allow players make random choice
    }
}