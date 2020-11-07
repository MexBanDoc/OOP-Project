using System.Collections.Generic;
using Mafia.Domain;

namespace Mafia.App
{
    public interface IBot
    {
        public IPerson GetPersonToHeal(IEnumerable<IPerson> doctor);

        public IPerson GetPersonToMurder(IEnumerable<IPerson> mafia);

        public IPerson GetPersonToInvestigate(IEnumerable<IPerson> sheriff);

        public void StarGame();
        
        // allow players make random choice
    }
}