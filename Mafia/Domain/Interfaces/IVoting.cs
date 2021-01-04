using System.Threading.Tasks;

namespace Mafia.Domain
{
    public interface IVoting
    {
        public Role Role { get; }

        public Task Start();
        public Task End();
        
        public IPerson Result { get; }
    }
}