using System.Collections.Generic;
using Mafia.Domain;

namespace Mafia.App
{
    public interface IPlayersPool
    {
        bool IsOpen { get; }
        bool AddPlayer(long playerId, string name);
        IEnumerable<(long, IPerson)> ExtractPersons();
    }
}