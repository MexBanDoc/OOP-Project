using System.Collections.Generic;
using Mafia.Domain;

namespace Mafia.App
{
    public interface IPlayersPool
    {
        bool AddPlayer(long playerId, string name);
        IEnumerable<(long, IPerson)> ExtractPersons(ISettings settings);
    }
}