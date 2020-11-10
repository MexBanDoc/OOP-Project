using System.Collections.Generic;
using Mafia.Domain;

namespace Mafia.App
{
    public class TGBot : IUserInterface
    {
        public IPerson GetPersonToHeal(IEnumerable<IPerson> doctor)
        {
            throw new System.NotImplementedException();
        }

        public IPerson GetPersonToMurder(IEnumerable<IPerson> mafia)
        {
            throw new System.NotImplementedException();
        }

        public IPerson GetPersonToInvestigate(IEnumerable<IPerson> sheriff)
        {
            throw new System.NotImplementedException();
        }

        public IPerson AskForTarget(IEnumerable<IPerson> players, Role role)
        {
            throw new System.NotImplementedException();
        }

        public void StarGame()
        {
            throw new System.NotImplementedException();
        }
    }
}