using System.Collections.Generic;
using System.Threading.Tasks;
using Mafia.Domain;

namespace Mafia.Domain
{
    public interface IUserInterface
    {
        public Task<IPerson> AskForInteractionTarget(IEnumerable<IPerson> players, Role role, ICity city);

        public Task TellResults(ICity city, DayTime dayTime);

        public Task TellGameResult(WinState state, ICity city);
    }
}