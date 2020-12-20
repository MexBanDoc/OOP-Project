using System.Collections.Generic;
using Mafia.Domain;

namespace Mafia.App
{
    public interface IPlayersPool
    {
        bool IsOpen { get; }
        bool AddPlayerAsync(long playerId);
        IEnumerable<KeyValuePair<long, IPerson>> ExtractPersons();
    }
}