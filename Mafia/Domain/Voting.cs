using System.Linq;
using System.Threading.Tasks;

namespace Mafia.Domain
{
    public class Voting : IVoting
    {
        public Role Role { get; }
        private readonly ICity city;
        private readonly IUserInterface userInterface;

        public Voting(Role role, ICity city, IUserInterface userInterface)
        {
            Role = role;
            this.city = city;
            this.userInterface = userInterface;
        }

        public async Task Start()
        {
            var band = city.Population
                .Where(person => person.IsAlive)
                .Where(person => (Role.DayTime == DayTime.Day ? person.DayRole : person.NightRole) == Role);
            await userInterface.AskForInteractionTarget(band, Role, city);
        }

        public async Task End()
        {
            Result = await userInterface.GetInteractionTarget(Role, city.Name);
        }

        public IPerson Result { get; private set; }
    }
}