using System.Collections.Generic;
using Mafia.Domain;
using Telegram.Bot.Args;

namespace Mafia.App
{
    public class TGBot : IUserInterface
    {
        /*
        public IPerson GetPersonToHeal(IEnumerable<IPerson> healers)
        {
            return AskForInteractionTarget(healers, new Healer());
        }

        public IPerson GetPersonToMurder(IEnumerable<IPerson> mafia)
        {
            throw new System.NotImplementedException();
        }

        public IPerson GetPersonToInvestigate(IEnumerable<IPerson> sheriff)
        {
            throw new System.NotImplementedException();
        }
        */
        public IPerson AskForInteractionTarget(IEnumerable<IPerson> players, Role role, ICity city)
        {
            throw new System.NotImplementedException();
        }

        public void StartGame()
        {
            throw new System.NotImplementedException();
        }

        public void TellResults(ICity city, DayTime dayTime)
        {
            throw new System.NotImplementedException();
        }


        public void TellGameResult(WinState state)
        {
            throw new System.NotImplementedException();
        }
    }
}