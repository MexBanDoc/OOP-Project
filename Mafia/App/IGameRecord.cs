using System.Collections.Generic;
using Mafia.Domain;

namespace Mafia.App
{
    public interface IGameRecord
    {
        public ISettings Settings { get; set; }
        bool AddPlayer(long playerId, string name);
        IEnumerable<(long, IPerson)> ExtractPersons();
    }
}