using System.Collections.Generic;
using Mafia.Domain;

namespace Mafia.Domain
{
    public interface IUserInterface
    {
        public IPerson AskForInteractionTarget(IEnumerable<IPerson> players, Role role, ICity city);

        public void TellResults(ICity city, DayTime dayTime);

        public void TellGameResult(WinState state, ICity city);
    }
}